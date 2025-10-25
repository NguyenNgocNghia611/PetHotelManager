namespace PetHotelManager.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class EmployeeProfile
{
    [Key]
    [ForeignKey("ApplicationUser")]
    public string Id { get; set; }

    public string?  Address  { get; set; }
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public string?  Position { get; set; }
    public decimal  Salary   { get; set; } = 0;

    // Navigation property để EF Core hiểu mối quan hệ
    public virtual ApplicationUser ApplicationUser { get; set; }
}