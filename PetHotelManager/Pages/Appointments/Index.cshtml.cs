using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Appointments;

[Authorize] // yêu cầu đăng nhập
public class IndexModel(IHttpClientFactory httpClientFactory) : PageModel
{
    private readonly IHttpClientFactory _http = httpClientFactory;

    public bool IsAdminStaffVet => User.IsInRole("Admin") || User.IsInRole("Staff") || User.IsInRole("Veterinarian") || User.IsInRole("Doctor");

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public string? LookupId { get; set; }

    public List<AppointmentRow> Items { get; set; } = [];
    public int TotalRecords { get; set; }
    public string? Error { get; set; }

    public AppointmentDetail? Lookup { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var client = _http.CreateClient();

            if (IsAdminStaffVet)
            {
                var url = $"{baseUrl}/api/Appointments?pageNumber={PageNumber}&pageSize={PageSize}";
                if (!string.IsNullOrWhiteSpace(Search))
                    url += $"&search={Uri.EscapeDataString(Search)}";

                var res = await client.GetFromJsonAsync<AppointmentsListResponse>(url);
                if (res != null)
                {
                    Items = res.data ?? [];
                    TotalRecords = res.pagination?.TotalRecords ?? 0;
                }
            }

            if (!string.IsNullOrWhiteSpace(LookupId))
            {
                if (int.TryParse(LookupId, out var id))
                {
                    var detailUrl = $"{baseUrl}/api/Appointments/{id}";
                    Lookup = await client.GetFromJsonAsync<AppointmentDetail>(detailUrl);
                }
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAccept(int id)
    {
        if (!IsAdminStaffVet) return Forbid();
        return await PostNoBody($"/api/Appointments/{id}/accept");
    }

    public async Task<IActionResult> OnPostReject(int id)
    {
        if (!IsAdminStaffVet) return Forbid();
        return await PostNoBody($"/api/Appointments/{id}/reject", method: HttpMethod.Put);
    }

    public async Task<IActionResult> OnPostCheckin(int id)
    {
        if (!IsAdminStaffVet) return Forbid();
        return await PostNoBody($"/api/Appointments/checkin/{id}");
    }

    public async Task<IActionResult> OnPostCheckout(int id)
    {
        if (!IsAdminStaffVet) return Forbid();
        return await PostNoBody($"/api/Appointments/checkout/{id}");
    }

    public async Task<IActionResult> OnPostCancel(int id)
    {
        // Customer/Staff đều có thể cancel; server enforce ownership
        return await PostNoBody($"/api/Appointments/{id}/cancel", method: HttpMethod.Put);
    }

    private async Task<IActionResult> PostNoBody(string relative, HttpMethod? method = null)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}{relative}";
            var client = _http.CreateClient();
            var req = new HttpRequestMessage(method ?? HttpMethod.Post, url)
            {
                Content = new StringContent("", System.Text.Encoding.UTF8, "application/json")
            };
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
        return RedirectToPage(new { Search, PageNumber, PageSize, LookupId });
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

    public sealed class Pagination
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    public sealed class AppointmentsListResponse
    {
        public List<AppointmentRow>? data { get; set; }
        public Pagination? pagination { get; set; }
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