using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetHotelManager.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoomNumber { get; set; }

        [ForeignKey(nameof(RoomType))]
        public int RoomTypeId { get; set; }
        public RoomType RoomType { get; set; }

        public string Status { get; set; }  // Available, Occupied, Maintenance

        // Navigation
        public ICollection<Appointment> Appointments { get; set; }
    }
}
