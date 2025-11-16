using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Appointments;

[Authorize] // Customer hoặc Staff
public class CreateModel(IHttpClientFactory httpClientFactory) : PageModel
{
    private readonly IHttpClientFactory _http = httpClientFactory;

    public bool IsCustomer => User.IsInRole("Customer");
    public bool IsStaff => User.IsInRole("Staff") || User.IsInRole("Admin") || User.IsInRole("Veterinarian") || User.IsInRole("Doctor");

    public List<ServiceItem> Services { get; set; } = [];
    public List<RoomItem> Rooms { get; set; } = [];

    [BindProperty]
    public CreateAppointmentForm Form { get; set; } = new();

    public string? Error { get; set; }
    public string? Success { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var client = _http.CreateClient();

            Services = await client.GetFromJsonAsync<List<ServiceItem>>($"{baseUrl}/api/Services") ?? [];
            Rooms = await client.GetFromJsonAsync<List<RoomItem>>($"{baseUrl}/api/Rooms") ?? [];
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var client = _http.CreateClient();

            var dto = new CreateAppointmentDto
            {
                UserId = IsCustomer
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
                    : (Form.UserId ?? string.Empty),
                PetId = Form.PetId,
                ServiceId = Form.ServiceId,
                RoomId = Form.RoomId,
                AppointmentDate = Form.AppointmentDate,
                CheckOutDate = Form.CheckOutDate,
                Notes = Form.Notes
            };

            var res = await client.PostAsJsonAsync($"{baseUrl}/api/Appointments/create", dto);
            if (res.IsSuccessStatusCode)
            {
                Success = "Đặt lịch thành công!";
                // reset form nhẹ
                Form = new CreateAppointmentForm();
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
        // reload dropdowns khi lỗi
        await OnGet();
        return Page();
    }

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
        public string? UserId { get; set; } // chỉ dùng cho Staff, Customer tự lấy từ claims
        public int PetId { get; set; }
        public int ServiceId { get; set; }
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