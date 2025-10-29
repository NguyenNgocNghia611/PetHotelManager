using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public decimal Price { get; set; }

        public string Unit { get; set; }

        // Navigation
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
