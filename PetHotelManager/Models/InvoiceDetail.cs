using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetHotelManager.Models
{
    public class InvoiceDetail
    {
        [Key]
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        [ForeignKey(nameof(InvoiceId))]
        public Invoice Invoice { get; set; }

        public int? ServiceId { get; set; }
        [ForeignKey(nameof(ServiceId))]
        public Service Service { get; set; }

        public int? ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}
