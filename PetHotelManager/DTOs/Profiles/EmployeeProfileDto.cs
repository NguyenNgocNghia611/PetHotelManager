namespace PetHotelManager.DTOs.Profiles;

public class EmployeeProfileDto
{
    public string   Id       { get; set; } // User Id
    public string?  Address  { get; set; }
    public DateTime HireDate { get; set; }
    public string?  Position { get; set; }
    public decimal  Salary   { get; set; }
}