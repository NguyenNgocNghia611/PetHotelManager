using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Models
{
    public class RoomType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TypeName { get; set; }

        public decimal PricePerDay { get; set; }

        public string? Description { get; set; }

        // Navigation
        public ICollection<Room> Rooms { get; set; }
    }
}
