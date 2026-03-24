using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models.Dto
{
    public class ProductCreateRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 999999.99)]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public IFormFile? ImageFile { get; set; }
    }

    public class ProductUpdateRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 999999.99)]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public IFormFile? ImageFile { get; set; }

        public bool RemoveImage { get; set; }
    }
}
