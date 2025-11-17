using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.RoomTypes
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

        public List<RoomTypeItem> Items { get; set; } = new();
        public string? Error { get; set; }
        public string? Success { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                var list = await client.GetFromJsonAsync<List<RoomTypeItem>>($"{baseUrl}/api/RoomTypes")
                           ?? new List<RoomTypeItem>();

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    var keyword = Search.Trim().ToLower();
                    list = list.Where(r => (r.TypeName ?? "").ToLower().Contains(keyword) || (r.Description ?? "").ToLower().Contains(keyword)).ToList();
                }

                Items = list.OrderBy(r => r.TypeName).ToList();
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

                var res = await client.DeleteAsync($"{baseUrl}/api/RoomTypes/{id}");
                if (res.IsSuccessStatusCode)
                {
                    Success = "Xóa loại phòng thành công.";
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

        public class RoomTypeItem
        {
            public int Id { get; set; }
            public string TypeName { get; set; } = "";
            public decimal PricePerDay { get; set; }
            public string? Description { get; set; }
        }
    }
}