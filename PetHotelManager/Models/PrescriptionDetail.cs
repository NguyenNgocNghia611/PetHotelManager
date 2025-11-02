using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetHotelManager.Models
{
    public class PrescriptionDetail
    {
        [Key]
        public int Id { get; set; }

        public int MedicalRecordId { get; set; }
        [ForeignKey(nameof(MedicalRecordId))]
        public MedicalRecord MedicalRecord { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        public string Dosage { get; set; }
        public int Quantity { get; set; }
    }
}
