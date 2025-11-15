using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Product
{
    /// <summary>
    /// DTO cho từng dòng sản phẩm trong phiếu nhập
    /// </summary>
    public class InventoryReceiptLineDto
    {
        [Required(ErrorMessage = "ProductId không được để trống")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        /// <summary>
        /// Giá nhập (optional)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập phải >= 0")]
        public decimal? UnitPrice { get; set; }

        public string? Notes { get; set; }
    }
}
