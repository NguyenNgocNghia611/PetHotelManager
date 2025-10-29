using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetHotelManager.Models
{
    public class MedicalRecord
    {
        [Key]
        public int Id { get; set; }

        public int PetId { get; set; }
        [ForeignKey(nameof(PetId))]
        public Pet Pet { get; set; }

        public string VeterinarianId { get; set; } 
        [ForeignKey(nameof(VeterinarianId))]
        public ApplicationUser Veterinarian { get; set; }

        public DateTime ExaminationDate { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }

        // Navigation
        public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; }
    }
}
