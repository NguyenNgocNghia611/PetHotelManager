namespace PetHotelManager.Services;

using PetHotelManager.DTOs.Dashboard;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}