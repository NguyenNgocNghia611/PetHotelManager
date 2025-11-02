using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Dashboard;
using PetHotelManager.Services;

namespace PetHotelManager.Pages.Admin.Dashboard
{
    using PetHotelManager.DTOs.Dashboard;

    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public IndexModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public DashboardStatsDto Stats { get; set; } = new DashboardStatsDto();

        public async Task<IActionResult> OnGetAsync()
        {
            Stats = await _dashboardService.GetDashboardStatsAsync();

            return Page();
        }
    }
}