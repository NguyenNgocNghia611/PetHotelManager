namespace PetHotelManager.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int     TotalCustomers    { get; set; }
    public int     TotalPets         { get; set; }
    public int     AppointmentsToday { get; set; }
    public decimal RevenueToday      { get; set; }
}