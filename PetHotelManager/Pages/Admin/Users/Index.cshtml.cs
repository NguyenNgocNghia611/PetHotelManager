using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Admin;
using System.Text.Json;

namespace PetHotelManager.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IList<UserManagementDto> UserList { get; set; } = new List<UserManagementDto>();

        private async Task<HttpClient> GetAuthenticatedClientAsync()
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
            var client  = await GetAuthenticatedClientAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = await client.GetAsync($"{baseUrl}/api/admin/users");

            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                UserList = await JsonSerializer.DeserializeAsync<List<UserManagementDto>>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(string id)
        {
            var client  = await GetAuthenticatedClientAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = await client.PutAsync($"{baseUrl}/api/admin/users/{id}/toggle-status", null);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var client  = await GetAuthenticatedClientAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = await client.DeleteAsync($"{baseUrl}/api/admin/users/{id}");

            return RedirectToPage();
        }
    }
}