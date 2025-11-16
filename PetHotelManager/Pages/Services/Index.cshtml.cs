using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Services
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

        public List<ServiceItem> Items { get; set; } = new();
        public string? Error { get; set; }
        public string? Success { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client = _http.CreateClient();

                var list = await client.GetFromJsonAsync<List<ServiceItem>>($"{baseUrl}/api/Services")
                           ?? new List<ServiceItem>();

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    var keyword = Search.Trim().ToLower();
                    list = list.Where(s => s.Name.ToLower().Contains(keyword) || s.Category.ToLower().Contains(keyword)).ToList();
                }

                Items = list.OrderBy(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return Page();
        }

        // Xóa (gọi endpoint DELETE) – xác nhận đơn giản
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client = _http.CreateClient();

                // Sao chép cookie để đảm bảo xác thực (nếu không sẵn)
                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var res = await client.DeleteAsync($"{baseUrl}/api/Services/{id}");
                if (res.IsSuccessStatusCode)
                {
                    Success = "Xóa dịch vụ thành công.";
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

        public class ServiceItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Category { get; set; } = "";
            public decimal Price { get; set; }
            public string Unit { get; set; } = "";
        }
    }
}