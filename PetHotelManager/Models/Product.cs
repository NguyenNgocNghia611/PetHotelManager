using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public string Unit { get; set; }

        // Navigation
        public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; }
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }
}
