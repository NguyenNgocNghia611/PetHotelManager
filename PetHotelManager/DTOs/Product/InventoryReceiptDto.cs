using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Product
{
    /// <summary>
    /// DTO để tạo phiếu nhập kho - F7.1
    /// </summary>
    public class InventoryReceiptDto
    {
        /// <summary>
        /// Tên nhà cung cấp
        /// </summary>
        [Required(ErrorMessage = "Nhà cung cấp không được để trống")]
        [StringLength(200, ErrorMessage = "Tên nhà cung cấp không được vượt quá 200 ký tự")]
        public string Supplier { get; set; }

        /// <summary>
        /// Ngày nhập (mặc định hôm nay)
        /// </summary>
        public DateTime? ReceiptDate { get; set; }

        /// <summary>
        /// Danh sách sản phẩm nhập
        /// </summary>
        [Required(ErrorMessage = "Phải có ít nhất 1 sản phẩm")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 sản phẩm")]
        public List<InventoryReceiptLineDto> Lines { get; set; } = new();

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Notes { get; set; }
    }

    
}