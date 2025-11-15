using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Product
{
    /// <summary>
    /// DTO để cập nhật sản phẩm
    /// </summary>
    public class UpdateProductDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        [StringLength(50, ErrorMessage = "Đơn vị tính không được vượt quá 50 ký tự")]
        public string Unit { get; set; }

        // ⭐ THÊM MỚI - F7

        [Range(0, int.MaxValue, ErrorMessage = "Mức tối thiểu phải >= 0")]
        public int MinimumStock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Mức đặt hàng phải >= 0")]
        public int ReorderLevel { get; set; }

        [StringLength(50, ErrorMessage = "Danh mục không được vượt quá 50 ký tự")]
        public string? Category { get; set; }

        public bool IsActive { get; set; }
    }
}