using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;

namespace PetHotelManager.Pages.Appointments
{
    [Authorize(Roles = "Admin,Staff,Veterinarian,Doctor")]
    public class FilterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FilterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } = "Pending";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public List<AppointmentRow>? Items { get; set; }
        public int TotalItems { get; set; }
        public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Pet)
                    .Include(a => a.Service)
                    .Include(a => a.Room)
                    .Where(a => a.Status == Status);

                TotalItems = await query.CountAsync();

                Items = await query
                    .OrderByDescending(a => a.AppointmentDate)
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .Select(a => new AppointmentRow
                    {
                        Id = a.Id,
                        CustomerName = a.User.FullName,
                        PetName = a.Pet.Name,
                        ServiceName = a.Service.Name,
                        RoomName = a.Room != null ? a.Room.RoomNumber : null,
                        AppointmentDate = a.AppointmentDate,
                        Status = a.Status
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }

        public class AppointmentRow
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public string PetName { get; set; } = "";
            public string ServiceName { get; set; } = "";
            public string? RoomName { get; set; }
            public DateTime AppointmentDate { get; set; }
            public string Status { get; set; } = "";
        }
    }
}