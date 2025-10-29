using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetHotelManager.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } // FK -> AspNetUsers.Id
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public DateTime InvoiceDate { get; set; }

        // Navigation
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }
}
