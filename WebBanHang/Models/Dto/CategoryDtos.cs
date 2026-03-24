using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models.Dto
{
    public class CategoryRequest
    {
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }

    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
