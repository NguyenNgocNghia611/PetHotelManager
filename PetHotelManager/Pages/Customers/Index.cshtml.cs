// File: Pages/Customers/Index.cshtml.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Models;
using System.Text.Json;

namespace PetHotelManager.Pages.Customers
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IList<ApplicationUser> CustomerList { get; set; } = new List<ApplicationUser>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token  = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }

            var baseUrl    = $"{Request.Scheme}://{Request.Host}";
            var requestUrl = $"{baseUrl}/api/customermanagement/list";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                requestUrl += $"?keyword={SearchTerm}";
            }

            var response = await client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                CustomerList = await JsonSerializer.DeserializeAsync<List<ApplicationUser>>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
    }
}