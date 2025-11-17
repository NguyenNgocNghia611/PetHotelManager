using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using PetHotelManager.DTOs.Appointments;

namespace PetHotelManager.Pages.Veterinarian.Appointments
{
    [Authorize(Roles = "Veterinarian")]
    public class MyModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public MyModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<AppointmentWithReceiver> MyAppointments { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies["ApiToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(currentUserId))
                {
                    ErrorMessage = "Không thể xác định người dùng hiện tại.";
                    return;
                }

                // Get paginated appointments list
                var listResponse = await client.GetFromJsonAsync<AppointmentsListResponse>(
                    $"{baseUrl}/api/appointments?pageNumber={PageNumber}&pageSize={PageSize}");

                if (listResponse == null || listResponse.Data == null)
                {
                    ErrorMessage = "Không thể tải danh sách lịch hẹn.";
                    return;
                }

                // Filter appointments assigned to current veterinarian
                // Due to API limitation, we need to fetch details for each appointment
                var myAppointments = new List<AppointmentWithReceiver>();

                foreach (var appointment in listResponse.Data)
                {
                    // Fetch appointment details to get ReceiverId
                    try
                    {
                        var detailResponse = await client.GetAsync($"{baseUrl}/api/appointments/{appointment.Id}");
                        if (detailResponse.IsSuccessStatusCode)
                        {
                            var detail = await detailResponse.Content.ReadFromJsonAsync<AppointmentDetailWithReceiver>();
                            if (detail != null && detail.ReceiverId == currentUserId)
                            {
                                myAppointments.Add(new AppointmentWithReceiver
                                {
                                    Id = appointment.Id,
                                    CustomerName = appointment.CustomerName,
                                    PetName = appointment.PetName,
                                    ServiceName = appointment.ServiceName,
                                    RoomName = appointment.RoomName,
                                    AppointmentDate = appointment.AppointmentDate,
                                    Status = appointment.Status,
                                    ReceiverId = detail.ReceiverId
                                });
                            }
                        }
                    }
                    catch
                    {
                        // Skip appointments we can't fetch details for
                        continue;
                    }
                }

                MyAppointments = myAppointments;

                // Update pagination info (note: this is approximate since we filter client-side)
                if (listResponse.Pagination != null)
                {
                    TotalRecords = MyAppointments.Count; // Only count filtered results
                    TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostAcceptAsync(int id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var response = await client.PutAsync($"{baseUrl}/api/appointments/{id}/accept", null);
                
                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Đã chấp nhận lịch hẹn thành công!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Lỗi khi chấp nhận lịch hẹn: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
            }

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            try
            {
                var client = GetAuthenticatedClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var response = await client.PutAsync($"{baseUrl}/api/appointments/{id}/reject", null);
                
                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Đã từ chối lịch hẹn thành công!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Lỗi khi từ chối lịch hẹn: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
            }

            await OnGetAsync();
            return Page();
        }

        // DTOs
        public class AppointmentsListResponse
        {
            public List<AppointmentListDto>? Data { get; set; }
            public PaginationInfo? Pagination { get; set; }
        }

        public class PaginationInfo
        {
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalRecords { get; set; }
            public int TotalPages { get; set; }
        }

        public class AppointmentWithReceiver
        {
            public int Id { get; set; }
            public string? CustomerName { get; set; }
            public string? PetName { get; set; }
            public string? ServiceName { get; set; }
            public string? RoomName { get; set; }
            public DateTime AppointmentDate { get; set; }
            public string? Status { get; set; }
            public string? ReceiverId { get; set; }
        }

        public class AppointmentDetailWithReceiver
        {
            public int Id { get; set; }
            public string? CustomerName { get; set; }
            public string? CustomerPhone { get; set; }
            public string? PetName { get; set; }
            public string? ServiceName { get; set; }
            public string? RoomName { get; set; }
            public DateTime AppointmentDate { get; set; }
            public string? Status { get; set; }
            public string? Notes { get; set; }
            public string? ReceiverId { get; set; }
        }
    }
}
