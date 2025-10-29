using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        public string RoomNumber { get; set; }

        public string TypeName { get; set; }

        public decimal PricePerDay { get; set; }

        public string Status { get; set; }

        // Navigation
        public ICollection<Appointment> Appointments { get; set; }
    }
}
