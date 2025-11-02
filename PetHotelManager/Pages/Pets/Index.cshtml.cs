using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Pets;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Pets
{
    [Authorize(Roles = "Admin,Staff,Doctor")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IList<PetListDto> PetList { get; set; } = new List<PetListDto>();

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token  = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }
            return client;
        }

        public async Task OnGetAsync()
        {
            var client  = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            PetList = await client.GetFromJsonAsync<List<PetListDto>>($"{baseUrl}/api/pets") ?? new List<PetListDto>();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client  = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            await client.DeleteAsync($"{baseUrl}/api/pets/{id}");
            return RedirectToPage();
        }
    }
}