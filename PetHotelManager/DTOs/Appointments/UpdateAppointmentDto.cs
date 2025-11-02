using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Appointments
{
    public class UpdateAppointmentDto
    {
        [Required]
        public int Id { get; set; }

        public string? Status { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? Notes { get; set; }
    }
}
