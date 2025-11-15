using Microsoft.AspNetCore.Authorization;
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
    public class AlertsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public AlertsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public int OutOfStockCount { get; set; }
        public int CriticalCount { get; set; }
        public int WarningCount { get; set; }
        public List<ProductDto> OutOfStockProducts { get; set; } = new();
        public List<ProductDto> CriticalProducts { get; set; } = new();
        public List<ProductDto> WarningProducts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var allProducts = await client.GetFromJsonAsync<List<ProductDto>>($"{baseUrl}/api/products")
                              ?? new List<ProductDto>();

            // Phân loại theo mức độ nghiêm trọng
            OutOfStockProducts = allProducts.Where(p => p.StockStatus == "OutOfStock").ToList();
            CriticalProducts = allProducts.Where(p => p.StockStatus == "Critical").ToList();
            WarningProducts = allProducts.Where(p => p.StockStatus == "Warning").ToList();

            OutOfStockCount = OutOfStockProducts.Count;
            CriticalCount = CriticalProducts.Count;
            WarningCount = WarningProducts.Count;
        }
    }
}