using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using System.Security.Claims;

namespace PetHotelManager.Pages.Appointments
{
    [Authorize] // quyền chi tiết đã enforce ở controller trước, đây yêu cầu đăng nhập
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool IsAdminStaffVet => User.IsInRole("Admin") || User.IsInRole("Staff") || User.IsInRole("Veterinarian");
        public bool IsCustomer => User.IsInRole("Customer");

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string? LookupId { get; set; }

        public List<AppointmentRow> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public string? Error { get; set; }
        public AppointmentDetail? Lookup { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                if (IsAdminStaffVet)
                {
                    var query = _context.Appointments
                        .Include(a => a.User)
                        .Include(a => a.Pet)
                        .Include(a => a.Service)
                        .Include(a => a.Room)
                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(Search))
                    {
                        var keyword = Search.Trim().ToLower();
                        query = query.Where(a =>
                            a.User.FullName.ToLower().Contains(keyword) ||
                            a.Pet.Name.ToLower().Contains(keyword) ||
                            a.Service.Name.ToLower().Contains(keyword));
                    }

                    TotalRecords = await query.CountAsync();

                    var data = await query
                        .OrderByDescending(a => a.AppointmentDate)
                        .Skip((PageNumber - 1) * PageSize)
                        .Take(PageSize)
                        .Select(a => new AppointmentRow
                        {
                            Id = a.Id,
                            CustomerName = a.User.FullName,
                            PetName = a.Pet.Name,
                            ServiceName = a.Service.Name,
                            RoomName = a.Room != null
                                ? $"{a.Room.RoomNumber} - {a.Room.RoomType.TypeName}"
                                : null,
                            AppointmentDate = a.AppointmentDate,
                            Status = a.Status
                        })
                        .ToListAsync();

                    Items = data;
                }

                if (!string.IsNullOrWhiteSpace(LookupId) && int.TryParse(LookupId, out var id))
                {
                    // Ownership check cho Customer
                    var appointmentQuery = _context.Appointments
                        .Include(a => a.User)
                        .Include(a => a.Pet)
                        .Include(a => a.Service)
                        .Include(a => a.Room).ThenInclude(r => r.RoomType)
                        .Where(a => a.Id == id);

                    if (IsCustomer)
                    {
                        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        appointmentQuery = appointmentQuery.Where(a => a.UserId == currentUserId);
                    }

                    Lookup = await appointmentQuery
                        .Select(a => new AppointmentDetail
                        {
                            Id = a.Id,
                            CustomerName = a.User.FullName,
                            CustomerPhone = a.User.PhoneNumber,
                            PetName = a.Pet.Name,
                            ServiceName = a.Service.Name,
                            RoomName = a.Room != null
                                ? $"{a.Room.RoomNumber} - {a.Room.RoomType.TypeName}"
                                : null,
                            AppointmentDate = a.AppointmentDate,
                            Status = a.Status,
                            Notes = a.Notes
                        })
                        .FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return Page();
        }

        // POST hành động (accept/reject/checkin/checkout/cancel) vẫn gọi API vì chứa nghiệp vụ và audit
        public async Task<IActionResult> OnPostAcceptAsync(int id) => await ProxyActionAsync($"/api/Appointments/{id}/accept", HttpMethod.Put);
        public async Task<IActionResult> OnPostRejectAsync(int id) => await ProxyActionAsync($"/api/Appointments/{id}/reject", HttpMethod.Put);
        public async Task<IActionResult> OnPostCheckinAsync(int id) => await ProxyActionAsync($"/api/Appointments/checkin/{id}", HttpMethod.Post);
        public async Task<IActionResult> OnPostCheckoutAsync(int id) => await ProxyActionAsync($"/api/Appointments/checkout/{id}", HttpMethod.Post);
        public async Task<IActionResult> OnPostCancelAsync(int id) => await ProxyActionAsync($"/api/Appointments/{id}/cancel", HttpMethod.Put);

        private async Task<IActionResult> ProxyActionAsync(string relative, HttpMethod method)
        {
            try
            {
                var client = new HttpClient(); // tạm thời; nếu cần dùng IHttpClientFactory có thể giữ, action không deserialize JSON
                var url = $"{Request.Scheme}://{Request.Host}{relative}";

                // Sao chép cookie (để action được xác thực) – tối giản
                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
                }

                var req = new HttpRequestMessage(method, url);
                req.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                var res = await client.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    Error = $"Lỗi: {(int)res.StatusCode} - {await res.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return RedirectToPage(new
            {
                Search,
                PageNumber,
                PageSize,
                LookupId
            });
        }

        public sealed class AppointmentRow
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public string PetName { get; set; } = "";
            public string ServiceName { get; set; } = "";
            public string? RoomName { get; set; }
            public DateTime AppointmentDate { get; set; }
            public string Status { get; set; } = "";
        }

        public sealed class AppointmentDetail
        {
            public int Id { get; set; }
            public string CustomerName { get; set; } = "";
            public string? CustomerPhone { get; set; }
            public string PetName { get; set; } = "";
            public string ServiceName { get; set; } = "";
            public string? RoomName { get; set; }
            public DateTime AppointmentDate { get; set; }
            public string Status { get; set; } = "";
            public string? Notes { get; set; }
        }
    }
}