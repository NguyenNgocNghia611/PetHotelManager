namespace PetHotelManager.DTOs.Admin;

using System.ComponentModel.DataAnnotations;

public class AdminUpdateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string FullName { get;                set; }
    public            string? PhoneNumber { get; set; }
    [Required] public string  Role        { get; set; }

}