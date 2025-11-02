using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetHotelManager.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        // Dương = nhập, Âm = xuất
        public int ChangeQuantity { get; set; }

        // "Receipt", "Adjustment", ...
        [StringLength(50)]
        public string TransactionType { get; set; }

        // Optional: tham chiếu (ReceiptId, SupplierInvoiceId, ...)
        public int? ReferenceId { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedByUserId { get; set; }
    }
}