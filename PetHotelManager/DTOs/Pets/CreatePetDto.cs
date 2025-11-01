using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Pets
{
    public class CreatePetDto
    {
        [Required]
        public string UserId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Species { get; set; } = string.Empty;

        [StringLength(50)]
        public string Breed { get; set; } = string.Empty;

        [Range(0, 100)]
        public int Age { get; set; } = 0;

        [StringLength(30)]
        public string Color { get; set; } = string.Empty;

        [StringLength(255)]
        public string HealthStatus { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }
    }
}
