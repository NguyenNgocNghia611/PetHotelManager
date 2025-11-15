using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetHotelManager.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }

        // ===== PRODUCT REFERENCE =====

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        // ===== TRANSACTION INFO =====

        [Required]
        public int ChangeQuantity { get; set; }

        /// <summary>
        /// Loại giao dịch
        /// - "Receipt": Nhập kho từ NCC (F7.1)
        /// - "Sale": Xuất kho - Bán hàng (F7.2b)
        /// - "MedicalPrescription": Xuất kho - Kê đơn (F7.2a)
        /// - "Issue": Xuất kho thủ công - Hàng hỏng/hết hạn (F7.2c)
        /// - "Adjustment": Điều chỉnh sau kiểm kê (Admin)
        /// Database: NVARCHAR(50) NOT NULL
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; }

        // ===== REFERENCE INFO =====

        /// <summary>
        /// ID tham chiếu đến entity gốc
        /// VD: InvoiceId, MedicalRecordId
        /// Database: INT NULL
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// ⭐ THÊM MỚI
        /// Loại entity tham chiếu
        /// - "Invoice": Khi bán hàng
        /// - "MedicalRecord": Khi kê đơn
        /// - "Manual": Khi nhập/xuất thủ công
        /// Database: NVARCHAR(50) NULL
        /// </summary>
        [StringLength(50)]
        public string? ReferenceType { get; set; }

        [StringLength(200)]
        public string? Supplier { get; set; }

        /// <summary>
        /// ⭐ THÊM MỚI
        /// Giá nhập/xuất (đơn giá)
        /// Dùng để tính giá vốn và báo cáo tài chính
        /// Database: DECIMAL(18,2) NULL
        /// </summary>
        public decimal? UnitPrice { get; set; }

        // ===== ADDITIONAL INFO =====

        public string? Notes { get; set; }

        // ===== AUDIT INFO =====

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public ApplicationUser? CreatedByUser { get; set; }
    }
}