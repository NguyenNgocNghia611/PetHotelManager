using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Products
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

        public List<ProductItem> Items { get; set; } = new();
        public string? Error { get; set; }
        public string? Success { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                // Forward cookie để API nhận diện Admin
                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var list = await client.GetFromJsonAsync<List<ProductItem>>($"{baseUrl}/api/Products")
                           ?? new List<ProductItem>();

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    var keyword = Search.Trim().ToLower();
                    list = list.Where(p =>
                        (p.Name ?? string.Empty).ToLower().Contains(keyword) ||
                        (p.Unit ?? string.Empty).ToLower().Contains(keyword)
                    ).ToList();
                }

                Items = list.OrderByDescending(p => p.IsActive).ThenBy(p => p.Name).ToList();
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

                var res = await client.DeleteAsync($"{baseUrl}/api/Products/{id}");
                if (res.IsSuccessStatusCode)
                {
                    Success = "Xóa sản phẩm thành công.";
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

        public class ProductItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string Unit { get; set; } = "";
            public bool IsActive { get; set; }
        }
    }
}