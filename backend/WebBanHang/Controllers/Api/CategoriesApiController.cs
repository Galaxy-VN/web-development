using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using WebBanHang.Models.Dto;
using WebBanHang.Repositories.Interfaces;

namespace WebBanHang.Controllers.Api
{
    /// <summary>
    /// REST API for category CRUD operations.
    /// </summary>
    [ApiController]
    [Route("api/categories")]
    public class CategoriesApiController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesApiController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Gets all categories.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return Ok(categories.Select(MapToResponse));
        }

        /// <summary>
        /// Gets a category by id.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryResponse>> GetById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            return Ok(MapToResponse(category));
        }

        /// <summary>
        /// Creates a category.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryResponse>> Create([FromBody] CategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var category = new Category
            {
                Name = request.Name
            };

            await _categoryRepository.AddAsync(category);

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, MapToResponse(category));
        }

        /// <summary>
        /// Updates a category by id.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            category.Name = request.Name;
            await _categoryRepository.UpdateAsync(category);

            return NoContent();
        }

        /// <summary>
        /// Deletes a category by id.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            await _categoryRepository.DeleteAsync(id);
            return NoContent();
        }

        private static CategoryResponse MapToResponse(Category category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}
