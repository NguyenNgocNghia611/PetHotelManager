namespace PetHotelManager.DTOs.Profiles;

using System.ComponentModel.DataAnnotations;

public class UpdateEmployeeProfileDto
{
    public                             string? Address  { get; set; }
    [Required]                  public string? Position { get; set; }
    [Range(0, double.MaxValue)] public decimal Salary   { get; set; }
}