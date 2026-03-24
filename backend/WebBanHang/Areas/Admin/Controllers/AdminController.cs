using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Yêu cầu tài khoản phải có role "Admin"
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
