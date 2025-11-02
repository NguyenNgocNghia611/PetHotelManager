namespace PetHotelManager.DTOs.Invoices;

using System.ComponentModel.DataAnnotations;

public class InvoiceCreateDto
{
    [Required]
    public string UserId { get; set; }

    [MinLength(1)]
    public List<InvoiceDetailCreateDto> Details { get; set; } = new List<InvoiceDetailCreateDto>();
}