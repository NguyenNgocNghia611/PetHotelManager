namespace PetHotelManager.DTOs.Inventory
{
    /// <summary>
    /// DTO cho thống kê tổng quan kho - F7.4
    /// </summary>
    public class InventoryStatsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockCount { get; set; }
        public int CriticalStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int TransactionsToday { get; set; }
        public int TransactionsThisMonth { get; set; }
    }
}