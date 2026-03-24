namespace WebBanHang.Models.Dto
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<ProductImageResponse> Images { get; set; } = new();
    }

    public class ProductImageResponse
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
