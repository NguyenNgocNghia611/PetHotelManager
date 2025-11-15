using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Models
{
    public class Product
    {
       
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; }

        // ===== QUẢN LÝ KHO (F7) - THÊM MỚI =====

        public int MinimumStock { get; set; } = 10;

        public int ReorderLevel { get; set; } = 20;

        
        [StringLength(50)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;

        // ===== NAVIGATION PROPERTIES =====

        public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; }
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }

        /// <summary>
        /// ⭐ THÊM MỚI - F7
        /// Danh sách giao dịch kho (audit trail)
        /// </summary>
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
    }
}