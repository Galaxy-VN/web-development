using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;
using WebBanHang.Models.Dto;
using WebBanHang.Repositories.Interfaces;

namespace WebBanHang.Controllers.Api
{
    /// <summary>
    /// REST API for product CRUD operations.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private const long MaxImageSizeBytes = 2 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductsApiController> _logger;

        public ProductsApiController(
            IProductRepository productRepository,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ProductsApiController> logger)
        {
            _productRepository = productRepository;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
        {
            var products = await _productRepository.GetAllAsync();
            var result = products.Select(MapToResponse);
            return Ok(result);
        }

        /// <summary>
        /// Gets product details by id.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductResponse>> GetById(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            return Ok(MapToResponse(product));
        }

        /// <summary>
        /// Creates a new product with optional main image.
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductResponse>> Create([FromForm] ProductCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Product creation validation failed. Errors: {@ModelStateErrors}", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                return ValidationProblem(ModelState);
            }

            if (!await CategoryExistsAsync(request.CategoryId))
            {
                ModelState.AddModelError(nameof(request.CategoryId), "Category does not exist.");
                return ValidationProblem(ModelState);
            }

            if (!TryValidateImageFile(request.ImageFile, out var imageValidationMessage))
            {
                ModelState.AddModelError(nameof(request.ImageFile), imageValidationMessage!);
                return ValidationProblem(ModelState);
            }

            string? imageUrl = null;
            if (request.ImageFile != null)
            {
                imageUrl = await SaveImageAsync(request.ImageFile);
            }

            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                Description = request.Description ?? string.Empty,
                CategoryId = request.CategoryId,
                ImageUrl = imageUrl
            };

            try
            {
                await _productRepository.AddAsync(product);
            }
            catch
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    DeleteImageFile(imageUrl);
                }

                throw;
            }

            var createdProduct = await _productRepository.GetByIdAsync(product.Id);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, MapToResponse(createdProduct ?? product));
        }

        /// <summary>
        /// Updates a product by id. You can replace the main image.
        /// </summary>
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Product update validation failed for ID {ProductId}. Errors: {@ModelStateErrors}", 
                    id, ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                return ValidationProblem(ModelState);
            }

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            if (!await CategoryExistsAsync(request.CategoryId))
            {
                ModelState.AddModelError(nameof(request.CategoryId), "Category does not exist.");
                return ValidationProblem(ModelState);
            }

            if (!TryValidateImageFile(request.ImageFile, out var imageValidationMessage))
            {
                ModelState.AddModelError(nameof(request.ImageFile), imageValidationMessage!);
                return ValidationProblem(ModelState);
            }

            var oldImageUrl = existingProduct.ImageUrl;
            string? newImageUrl = null;

            if (request.ImageFile != null)
            {
                newImageUrl = await SaveImageAsync(request.ImageFile);
            }

            existingProduct.Name = request.Name;
            existingProduct.Price = request.Price;
            existingProduct.Description = request.Description ?? string.Empty;
            existingProduct.CategoryId = request.CategoryId;

            if (request.RemoveImage && !string.IsNullOrEmpty(existingProduct.ImageUrl))
            {
                existingProduct.ImageUrl = null;
            }

            if (!string.IsNullOrEmpty(newImageUrl))
            {
                existingProduct.ImageUrl = newImageUrl;
            }

            try
            {
                await _productRepository.UpdateAsync(existingProduct);
            }
            catch
            {
                if (!string.IsNullOrEmpty(newImageUrl))
                {
                    DeleteImageFile(newImageUrl);
                }

                throw;
            }

            if (!string.IsNullOrEmpty(newImageUrl) && !string.IsNullOrEmpty(oldImageUrl))
            {
                DeleteImageFile(oldImageUrl);
            }

            if (request.RemoveImage && string.IsNullOrEmpty(newImageUrl) && !string.IsNullOrEmpty(oldImageUrl))
            {
                DeleteImageFile(oldImageUrl);
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a product by id and removes related image files.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            var imageUrls = new List<string>();
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                imageUrls.Add(product.ImageUrl);
            }

            if (product.Images != null)
            {
                imageUrls.AddRange(product.Images.Where(i => !string.IsNullOrEmpty(i.Url)).Select(i => i.Url));
            }

            await _productRepository.DeleteAsync(id);

            foreach (var imageUrl in imageUrls.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                DeleteImageFile(imageUrl);
            }

            return NoContent();
        }

        private async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.Id == categoryId);
        }

        private static bool TryValidateImageFile(IFormFile? file, out string? message)
        {
            message = null;
            if (file == null || file.Length == 0)
            {
                return true;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                message = "Only .jpg, .jpeg, .png, and .webp files are allowed.";
                return false;
            }

            if (file.Length > MaxImageSizeBytes)
            {
                message = "Image file size must be 2MB or less.";
                return false;
            }

            return true;
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsDir);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/images/products/" + fileName;
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            var trimmedPath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, trimmedPath);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        private static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Images = product.Images?.Select(i => new ProductImageResponse
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList() ?? new List<ProductImageResponse>()
            };
        }
    }
}
