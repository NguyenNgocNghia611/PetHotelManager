using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Customers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public CustomerHistoryViewModel CustomerHistory { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null) return NotFound();

            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = await client.GetAsync($"{baseUrl}/api/customermanagement/{id}/history");

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                CustomerHistory = await JsonSerializer.DeserializeAsync<CustomerHistoryViewModel>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (CustomerHistory?.Customer == null)
                {
                    return NotFound();
                }

                return Page();
            }

            return NotFound();
        }

        public class CustomerHistoryViewModel
        {
            public CustomerInfo Customer { get; set; }
            public List<PetInfo> Pets { get; set; } = new List<PetInfo>();
            public List<InvoiceInfo> Invoices { get; set; } = new List<InvoiceInfo>();
        }

        public class CustomerInfo
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class PetInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Species { get; set; }
            public string Breed { get; set; }
        }

        public class InvoiceInfo
        {
            public int Id { get; set; }
            public DateTime InvoiceDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
        }
    }
}