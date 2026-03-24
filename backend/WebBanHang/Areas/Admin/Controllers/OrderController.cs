using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).ThenInclude(od => od.Product).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}