using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Product
{
    public class UpdateProductDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        [StringLength(20, ErrorMessage = "Đơn vị tính không được vượt quá 20 ký tự")]
        public string Unit { get; set; }
    }
}
