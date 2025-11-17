using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Rooms
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public List<RoomItem> Items { get; set; } = new();
        public string? Error { get; set; }
        public string? Success { get; set; }

        // Trạng thái đề xuất
        public static readonly string[] StatusOptions = new[] { "Available", "Occupied", "Cleaning", "Maintenance" };

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                var list = await client.GetFromJsonAsync<List<RoomItem>>($"{baseUrl}/api/Rooms")
                           ?? new List<RoomItem>();

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    var keyword = Search.Trim().ToLower();
                    list = list.Where(r =>
                        (r.RoomNumber ?? "").ToLower().Contains(keyword) ||
                        (r.RoomTypeName ?? "").ToLower().Contains(keyword) ||
                        (r.Status ?? "").ToLower().Contains(keyword)
                    ).ToList();
                }

                Items = list.OrderBy(r => r.RoomNumber).ToList();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var res = await client.DeleteAsync($"{baseUrl}/api/Rooms/{id}");
                if (res.IsSuccessStatusCode)
                {
                    Success = "Xóa phòng thành công.";
                }
                else
                {
                    Error = $"Xóa thất bại: {(int)res.StatusCode} - {await res.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return RedirectToPage(new { Search });
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                // API yêu cầu body là string status (JSON)
                var res = await client.PutAsJsonAsync($"{baseUrl}/api/Rooms/update-status/{id}", status);
                if (res.IsSuccessStatusCode)
                {
                    Success = "Cập nhật trạng thái phòng thành công.";
                }
                else
                {
                    Error = $"Cập nhật thất bại: {(int)res.StatusCode} - {await res.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return RedirectToPage(new { Search });
        }

        public class RoomItem
        {
            public int Id { get; set; }
            public string RoomNumber { get; set; } = "";
            public string RoomTypeName { get; set; } = "";
            public decimal PricePerDay { get; set; }
            public string Status { get; set; } = "";
        }
    }
}