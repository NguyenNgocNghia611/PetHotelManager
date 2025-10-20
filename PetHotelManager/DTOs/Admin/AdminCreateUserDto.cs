namespace PetHotelManager.DTOs.Admin;

using System.ComponentModel.DataAnnotations;

public class AdminCreateUserDto
{
    [Required]                public string  Username    { get; set; }
    [Required] [EmailAddress] public string  Email       { get; set; }
    [Required]                public string  Password    { get; set; }
    [Required]                public string  FullName    { get; set; }
    [Required]                public string  Role        { get; set; } // "Staff" or "Doctor"
    public                           string? PhoneNumber { get; set; }

}