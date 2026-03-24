using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models.Dto
{
    public class CartItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequest
    {
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class CartItemResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal LineTotal => Price * Quantity;
    }

    public class CartResponse
    {
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public int TotalQuantity { get; set; }
    }
}
