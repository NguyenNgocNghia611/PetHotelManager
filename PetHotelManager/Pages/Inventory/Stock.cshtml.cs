using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Inventory
{
    [Authorize(Roles = "Admin,Staff")]
    public class StockModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public StockModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public StockReportResponse StockReport { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

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
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var queryString = $"?search={Search ?? ""}&category={Category ?? ""}";
            StockReport = await client.GetFromJsonAsync<StockReportResponse>($"{baseUrl}/api/inventory/stock{queryString}")
                          ?? new StockReportResponse();
        }

        public class StockReportResponse
        {
            public int TotalProducts { get; set; }
            public decimal TotalValue { get; set; }
            public int LowStockCount { get; set; }
            public List<StockItem> Products { get; set; } = new List<StockItem>();
        }

        public class StockItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public string? Category { get; set; }
            public int CurrentStock { get; set; }
            public int MinimumStock { get; set; }
            public int ReorderLevel { get; set; }
            public string Unit { get; set; } = "";
            public decimal Price { get; set; }
            public decimal TotalValue { get; set; }
            public string StockStatus { get; set; } = "";
            public bool IsActive { get; set; }
        }
    }
}
