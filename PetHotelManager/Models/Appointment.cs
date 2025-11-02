using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetHotelManager.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } 
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public int PetId { get; set; }
        [ForeignKey(nameof(PetId))]
        public Pet Pet { get; set; }

        public int? ServiceId { get; set; }
        [ForeignKey(nameof(ServiceId))]
        public Service Service { get; set; }

        public int? RoomId { get; set; }
        [ForeignKey(nameof(RoomId))]
        public Room Room { get; set; }
        public string? ReceiverId { get; set; }
        [ForeignKey(nameof(ReceiverId))]
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } 
        public string Notes { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
    }
}
