using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetHotelManager.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } 

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

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

        public string? ImageUrl { get; set; }

        // Navigation
        public ICollection<MedicalRecord> MedicalRecords { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
