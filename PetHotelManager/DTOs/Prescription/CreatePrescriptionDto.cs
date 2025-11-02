using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Prescription
{
    public class CreatePrescriptionDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string? Dosage { get; set; }
    }
}