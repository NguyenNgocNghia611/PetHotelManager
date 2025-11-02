using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Dashboard;
using System.Text.Json;

namespace PetHotelManager.Pages.Admin.Dashboard
{
    using PetHotelManager.DTOs.Dashboard;

    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public DashboardStatsDto Stats { get; set; } = new DashboardStatsDto();

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var requestUrl = $"{baseUrl}/api/dashboard/stats";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                request.Headers.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }

            try
            {
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    Stats = await JsonSerializer.DeserializeAsync<DashboardStatsDto>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Lỗi khi gọi API: {response.StatusCode} - {errorContent}");
                    Stats = new DashboardStatsDto();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi mạng: {ex.Message}");
                Stats = new DashboardStatsDto();
            }

            return Page();
        }
    }
}