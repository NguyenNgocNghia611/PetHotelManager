using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Dashboard;

namespace PetHotelManager.Services
{
    using PetHotelManager.DTOs.Dashboard;

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var customerRole   = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            int totalCustomers = 0;
            if (customerRole != null)
            {
                totalCustomers = await _context.UserRoles.CountAsync(ur => ur.RoleId == customerRole.Id);
            }

            var totalPets = await _context.Pets.CountAsync();
            var appointmentsToday = await _context.Appointments
                .CountAsync(a => a.AppointmentDate.Date == today);

            var revenueToday = await _context.Invoices
                .Where(i => i.Status == "Paid" && i.InvoiceDate.Date == today)
                .SumAsync(i => i.TotalAmount);

            return new DashboardStatsDto
            {
                TotalCustomers    = totalCustomers,
                TotalPets         = totalPets,
                AppointmentsToday = appointmentsToday,
                RevenueToday      = revenueToday
            };
        }
    }
}