using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetHotelManager.DTOs.Product;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Inventory
{
    [Authorize(Roles = "Admin,Staff")]
    public class IssueModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IssueModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public SelectList Products { get; set; }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var products = await client.GetFromJsonAsync<List<ProductDto>>($"{baseUrl}/api/products")
                           ?? new List<ProductDto>();

            Products = new SelectList(products, "Id", "Name");
        }
    }
}
