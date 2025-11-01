using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Service
{
    public class CreateServiceDto
    {
        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự.")]
        public string Name { get; set; }

        [MaxLength(50, ErrorMessage = "Loại dịch vụ không được vượt quá 50 ký tự.")]
        public string Category { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
        public decimal Price { get; set; }

        [MaxLength(20, ErrorMessage = "Đơn vị không được vượt quá 20 ký tự.")]
        public string Unit { get; set; }
    }
}
