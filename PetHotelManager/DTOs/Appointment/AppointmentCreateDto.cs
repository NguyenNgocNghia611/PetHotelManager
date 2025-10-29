using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Appointment;

public class AppointmentCreateDto
{
    [Required]
    public int PetId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    public int? ServiceId { get; set; }
    public int? RoomId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}