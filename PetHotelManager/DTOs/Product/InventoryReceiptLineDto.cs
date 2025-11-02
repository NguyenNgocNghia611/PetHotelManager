namespace PetHotelManager.DTOs.Product
{
    public class InventoryReceiptLineDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
