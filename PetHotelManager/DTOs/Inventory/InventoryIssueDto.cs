using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Inventory
{
    /// <summary>
    /// DTO cho xuất kho thủ công (hàng hỏng/hết hạn) - F7.2c
    /// </summary>
    public class InventoryIssueDto
    {
        /// <summary>
        /// Lý do xuất: Damaged, Expired, Lost, Other
        /// </summary>
        [Required(ErrorMessage = "Lý do xuất kho không được để trống")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Phải có ít nhất 1 sản phẩm")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 sản phẩm")]
        public List<IssueLineDto> Lines { get; set; }

        public string? Notes { get; set; }
    }

    public class IssueLineDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        public string? Notes { get; set; }
    }
}