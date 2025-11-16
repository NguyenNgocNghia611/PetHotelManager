using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;

namespace PetHotelManager.Pages.Rooms
{
    [Authorize(Roles = "Admin,Staff,Veterinarian,Doctor")]
    public class StatusModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public StatusModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<RoomStatusView>? Rooms { get; set; }
        public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var rooms = await _context.Rooms
                    .Include(r => r.RoomType)
                    .OrderBy(r => r.RoomNumber)
                    .Select(r => new RoomStatusView
                    {
                        Id = r.Id,
                        RoomNumber = r.RoomNumber,
                        RoomTypeName = r.RoomType.TypeName,
                        PricePerDay = r.RoomType.PricePerDay,
                        Status = r.Status
                    })
                    .ToListAsync();

                // Gán thông tin lịch hẹn đang CheckedIn (nếu có)
                var checkedIn = await _context.Appointments
                    .Include(a => a.Pet)
                    .Include(a => a.User)
                    .Include(a => a.Room)
                    .Where(a => a.Status == "CheckedIn" && a.RoomId != null)
                    .ToListAsync();

                foreach (var r in rooms)
                {
                    var appt = checkedIn.FirstOrDefault(a => a.Room != null && a.Room.RoomNumber == r.RoomNumber);
                    if (appt != null)
                    {
                        r.CurrentAppointmentId = appt.Id;
                        r.PetName = appt.Pet.Name;
                        r.CustomerName = appt.User.FullName;
                        r.CheckInDate = appt.CheckInDate;
                    }
                }

                Rooms = rooms;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return Page();
        }

        public class RoomStatusView
        {
            public int Id { get; set; }
            public string RoomNumber { get; set; } = "";
            public string RoomTypeName { get; set; } = "";
            public decimal PricePerDay { get; set; }
            public string Status { get; set; } = "";
            public int? CurrentAppointmentId { get; set; }
            public string? PetName { get; set; }
            public string? CustomerName { get; set; }
            public DateTime? CheckInDate { get; set; }
        }
    }
}