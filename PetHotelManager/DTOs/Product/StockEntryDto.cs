namespace PetHotelManager.DTOs.Product
{
    public class StockEntryDto
    {
        public int ProductId { get; set; }
        // dương = nhập; âm = xuất
        public int Quantity { get; set; }
        public string? TransactionType { get; set; } // e.g. "Adjustment"
        public string? Notes { get; set; }
    }
}
