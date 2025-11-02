using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace PetHotelManager.Pages.Invoices
{
    [Authorize(Roles = "Admin,Staff")]
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public InvoiceDetailViewModel? Invoice { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7234/api/invoices/{id}"); // THAY ĐÚNG PORT

            var cookie = Request.Cookies[".AspNetCore.Identity.Application"];
            if (cookie != null)
            {
                request.Headers.Add("Cookie", $".AspNetCore.Identity.Application={cookie}");
            }

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                Invoice = await JsonSerializer.DeserializeAsync<InvoiceDetailViewModel>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (Invoice == null)
                {
                    return NotFound();
                }

                return Page();
            }

            return NotFound();
        }
    }

    public class InvoiceDetailViewModel
    {
        public int Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public CustomerViewModel Customer { get; set; }
        public List<ItemDetailViewModel> Details { get; set; }
    }

    public class CustomerViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    public class ItemDetailViewModel
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public string ItemName { get; set; }
    }
}