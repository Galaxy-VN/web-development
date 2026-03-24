using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using WebBanHang.Repositories.Interfaces;

namespace WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = (await _productRepository.GetAllAsync())
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToList();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
