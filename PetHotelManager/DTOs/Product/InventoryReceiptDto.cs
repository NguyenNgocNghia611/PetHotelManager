namespace PetHotelManager.DTOs.Product
{
    public class InventoryReceiptDto
    {
        public string? Supplier { get; set; }
        public List<InventoryReceiptLineDto> Lines { get; set; } = new();
    }
}
