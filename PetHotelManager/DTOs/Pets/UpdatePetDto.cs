using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Pets
{
    public class UpdatePetDto
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Species { get; set; }

        [StringLength(50)]
        public string Breed { get; set; }

        [Range(0, 100)]
        public int Age { get; set; }

        [StringLength(30)]
        public string Color { get; set; }

        [StringLength(255)]
        public string HealthStatus { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
