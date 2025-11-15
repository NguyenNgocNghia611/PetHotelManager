using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Product;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Products
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IList<ProductDto> ProductList { get; set; } = new List<ProductDto>();

        // ⭐ THÊM MỚI
        public int LowStockCount { get; set; }
        public decimal TotalValue { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }
            return client;
        }

        public async Task OnGetAsync()
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // Build query string
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                queryParams.Add($"search={Uri.EscapeDataString(SearchTerm)}");
            }
            if (!string.IsNullOrEmpty(CategoryFilter))
            {
                queryParams.Add($"category={Uri.EscapeDataString(CategoryFilter)}");
            }

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

            ProductList = await client.GetFromJsonAsync<List<ProductDto>>($"{baseUrl}/api/products{queryString}")
                          ?? new List<ProductDto>();

            // ⭐ Tính toán thống kê
            LowStockCount = ProductList.Count(p => p.StockStatus != "Normal");
            TotalValue = ProductList.Sum(p => p.StockQuantity * p.Price);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            await client.DeleteAsync($"{baseUrl}/api/products/{id}");

            return RedirectToPage();
        }
    }
}