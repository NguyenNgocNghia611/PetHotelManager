using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.RoomTypes;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.RoomTypes
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IList<RoomTypeDto> RoomTypeList { get; set; } = new List<RoomTypeDto>();

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
            RoomTypeList = await client.GetFromJsonAsync<List<RoomTypeDto>>($"{baseUrl}/api/roomtypes") ?? new List<RoomTypeDto>();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client  = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            await client.DeleteAsync($"{baseUrl}/api/roomtypes/{id}");

            return RedirectToPage();
        }
    }
}