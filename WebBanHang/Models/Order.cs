using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        [Required]
        public string OrderDate { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
        [Required]
        [Precision(18, 2)]
        public decimal TotalPrice { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }

        public ApplicationUser? User { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
