using Microsoft.AspNetCore.Mvc;
using WebBanHang.Repositories.Interfaces;

namespace WebBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: /Product
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // GET: /Product/Display/5
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
