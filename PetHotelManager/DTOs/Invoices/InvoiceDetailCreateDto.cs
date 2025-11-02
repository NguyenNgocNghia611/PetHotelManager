namespace PetHotelManager.DTOs.Invoices;

public class InvoiceDetailCreateDto
{
    public int? ServiceId { get; set; }
    public int? ProductId { get; set; }

    public int Quantity { get; set; }
}