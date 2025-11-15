namespace PetHotelManager.DTOs.Inventory
{
    /// <summary>
    /// DTO cho lịch sử giao dịch kho - F7.3
    /// </summary>
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        /// <summary>
        /// Số lượng thay đổi (+ nhập, - xuất)
        /// </summary>
        public int ChangeQuantity { get; set; }

        /// <summary>
        /// Loại: Receipt, Sale, MedicalPrescription, Issue, Adjustment
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Loại tham chiếu: Invoice, MedicalRecord, Manual
        /// </summary>
        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Supplier { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Notes { get; set; }
        public string? CreatedByUserName { get; set; }
    }
}