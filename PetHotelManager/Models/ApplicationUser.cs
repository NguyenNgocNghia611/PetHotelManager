namespace PetHotelManager.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string   FullName  { get; set; } = string.Empty;
    public bool     IsActive  { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Pet> Pets { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
}