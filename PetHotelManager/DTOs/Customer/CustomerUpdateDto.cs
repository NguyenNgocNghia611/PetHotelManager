using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Customer;

public class CustomerUpdateDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string FullName { get; set; }
    public string? PhoneNumber { get; set; }
}
