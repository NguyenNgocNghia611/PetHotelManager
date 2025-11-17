using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetHotelManager.Data;

namespace PetHotelManager.Pages.Appointments
{
    [Authorize] // Customer hoặc Staff/Admin/Vet đều có thể tạo
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        private readonly ApplicationDbContext _db;

        public CreateModel(IHttpClientFactory httpClientFactory, ApplicationDbContext db)
        {
            _http = httpClientFactory;
            _db   = db;
        }

        public bool IsCustomer => User.IsInRole("Customer");
        public bool IsStaff => User.IsInRole("Staff") || User.IsInRole("Admin") || User.IsInRole("Veterinarian") || User.IsInRole("Doctor");

        public List<ServiceItem> Services { get; set; } = [];
        public List<RoomItem> Rooms { get; set; } = [];

        // Dùng cho Customer
        public List<SelectListItem> MyPetsSelect { get; set; } = [];

        [BindProperty]
        public CreateAppointmentForm Form { get; set; } = new();

        public string? Error { get; set; }
        public string? Success { get; set; }

        public async Task<IActionResult> OnGet()
        {
            await LoadLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync();
                return Page();
            }

            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                var dto = new CreateAppointmentDto
                {
                    UserId = IsCustomer
                        ? (User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty)
                        : (Form.UserId ?? string.Empty),
                    PetId          = Form.PetId,
                    ServiceId      = Form.ServiceId,
                    RoomId         = Form.RoomId,
                    AppointmentDate= Form.AppointmentDate,
                    CheckOutDate   = Form.CheckOutDate,
                    Notes          = Form.Notes
                };

                // Forward cookie để API nhận diện user/role
                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var res = await client.PostAsJsonAsync($"{baseUrl}/api/Appointments/create", dto);
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Appointments/Index");
                }
                else
                {
                    Error = await res.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            await LoadLookupsAsync();
            return Page();
        }

        private async Task LoadLookupsAsync()
        {
            // Load Services & Rooms qua API
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var client  = _http.CreateClient();

            Services = await client.GetFromJsonAsync<List<ServiceItem>>($"{baseUrl}/api/Services") ?? [];
            Rooms    = await client.GetFromJsonAsync<List<RoomItem>>($"{baseUrl}/api/Rooms") ?? [];

            // Nếu là Customer thì load “thú cưng của tôi” từ DbContext (tránh lỗi cookie khi GET API)
            if (IsCustomer)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var pets = _db.Pets
                              .Where(p => p.UserId == userId)
                              .OrderBy(p => p.Name)
                              .Select(p => new { p.Id, p.Name, p.Species })
                              .ToList();

                MyPetsSelect = pets
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text  = $"{p.Name} ({p.Species})"
                    })
                    .ToList();
            }
        }

        // DTOs cho binding/view
        public sealed class ServiceItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Category { get; set; } = "";
            public decimal Price { get; set; }
            public string Unit { get; set; } = "";
        }

        public sealed class RoomItem
        {
            public int Id { get; set; }
            public string RoomNumber { get; set; } = "";
            public string RoomTypeName { get; set; } = "";
            public decimal PricePerDay { get; set; }
            public string Status { get; set; } = "";
        }

        public sealed class CreateAppointmentForm
        {
            // Chỉ Staff/Admin/Vet nhập tay; Customer tự lấy UserId từ Claims
            public string? UserId { get; set; }

            // Bắt buộc chọn Pet đối với cả Customer/Staff
            [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Hãy chọn thú cưng")]
            public int PetId { get; set; }

            [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Hãy chọn dịch vụ")]
            public int ServiceId { get; set; }

            [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Hãy chọn phòng")]
            public int RoomId { get; set; }

            public DateTime AppointmentDate { get; set; } = DateTime.Now.AddHours(1);
            public DateTime CheckOutDate { get; set; } = DateTime.Now.AddHours(2);
            public string? Notes { get; set; }
        }

        public sealed class CreateAppointmentDto
        {
            public string UserId { get; set; } = "";
            public int PetId { get; set; }
            public int ServiceId { get; set; }
            public int RoomId { get; set; }
            public DateTime AppointmentDate { get; set; }
            public DateTime CheckOutDate { get; set; }
            public string? Notes { get; set; }
        }
    }
}