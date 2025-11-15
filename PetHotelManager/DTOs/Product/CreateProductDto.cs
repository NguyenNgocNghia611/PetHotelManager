using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Product
{
    public class CreateProductDto
    {
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

        /// <summary>
        /// Mức tối thiểu - Cảnh báo nghiêm trọng khi < mức này
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Mức tối thiểu phải >= 0")]
        public int MinimumStock { get; set; } = 10;

        /// <summary>
        /// Mức đặt hàng lại - Cảnh báo nên đặt hàng khi < mức này
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Mức đặt hàng phải >= 0")]
        public int ReorderLevel { get; set; } = 20;

        /// <summary>
        /// Danh mục sản phẩm
        /// </summary>
        [StringLength(50, ErrorMessage = "Danh mục không được vượt quá 50 ký tự")]
        public string? Category { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}