using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebBanHang.Data;
using WebBanHang.Models;
using WebBanHang.Repositories.Interfaces;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // GET: Add
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Add
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile? imageFile, List<IFormFile>? imageFiles)
        {
            if (ModelState.IsValid)
            {
                // Ảnh chính
                if (imageFile != null && imageFile.Length > 0)
                {
                    product.ImageUrl = await SaveImageAsync(imageFile);
                }

                // Ảnh phụ
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    product.Images = new List<ProductImage>();
                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var url = await SaveImageAsync(file);
                            product.Images.Add(new ProductImage { Url = url });
                        }
                    }
                }

                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(
            Product product,
            IFormFile? imageFile,
            List<IFormFile>? imageFiles,
            bool removeMainImage,
            List<int>? removedImageIds)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(product.Id);
                if (existingProduct == null) return NotFound();

                // Cập nhật thông tin cơ bản
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                // Xóa ảnh chính nếu được chọn
                if (removeMainImage && !string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    DeleteImageFile(existingProduct.ImageUrl);
                    existingProduct.ImageUrl = null;
                }

                // Thay ảnh chính mới
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        DeleteImageFile(existingProduct.ImageUrl);
                    }
                    existingProduct.ImageUrl = await SaveImageAsync(imageFile);
                }

                // Xóa ảnh phụ được chọn
                if (removedImageIds != null && removedImageIds.Count > 0 && existingProduct.Images != null)
                {
                    var imagesToRemove = existingProduct.Images.Where(i => removedImageIds.Contains(i.Id)).ToList();
                    foreach (var img in imagesToRemove)
                    {
                        DeleteImageFile(img.Url);
                        _context.ProductImages.Remove(img);
                    }
                }

                // Thêm ảnh phụ mới
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    existingProduct.Images ??= new List<ProductImage>();
                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var url = await SaveImageAsync(file);
                            existingProduct.Images.Add(new ProductImage { Url = url, ProductId = existingProduct.Id });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                // Xóa ảnh chính
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteImageFile(product.ImageUrl);
                }
                // Xóa ảnh phụ
                if (product.Images != null)
                {
                    foreach (var img in product.Images)
                    {
                        DeleteImageFile(img.Url);
                    }
                }
            }
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // === Helper methods ===

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsDir);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/products/" + fileName;
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}