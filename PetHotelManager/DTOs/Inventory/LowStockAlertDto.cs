namespace PetHotelManager.DTOs.Inventory
{
    /// <summary>
    /// DTO cho cảnh báo tồn kho thấp - F7.4
    /// </summary>
    public class LowStockAlertDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? Category { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int ReorderLevel { get; set; }
        public string Unit { get; set; }

        /// <summary>
        /// Số lượng thiếu = ReorderLevel - CurrentStock
        /// </summary>
        public int ShortageQuantity => Math.Max(0, ReorderLevel - CurrentStock);

        /// <summary>
        /// Mức độ nghiêm trọng: Warning, Critical, OutOfStock
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Số lượng đề xuất đặt hàng
        /// </summary>
        public int SuggestedOrderQuantity { get; set; }
    }
}