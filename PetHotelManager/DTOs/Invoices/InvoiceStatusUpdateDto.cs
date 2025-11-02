namespace PetHotelManager.DTOs.Invoices;

using System.ComponentModel.DataAnnotations;

public class InvoiceStatusUpdateDto
{
    [Required]
    public string Status { get; set; }
}