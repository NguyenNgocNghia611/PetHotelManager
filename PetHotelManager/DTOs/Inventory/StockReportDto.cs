namespace PetHotelManager.DTOs.Inventory
{
    /// <summary>
    /// DTO cho báo cáo tồn kho hiện tại - F7.3
    /// </summary>
    public class StockReportDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? Category { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int ReorderLevel { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }

        /// <summary>
        /// Giá trị tồn kho = CurrentStock × Price
        /// </summary>
        public decimal TotalValue => CurrentStock * Price;

        /// <summary>
        /// Trạng thái: Normal, Warning, Critical, OutOfStock
        /// </summary>
        public string StockStatus { get; set; }

        public bool IsActive { get; set; }
    }
}