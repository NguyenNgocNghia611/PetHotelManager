using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Appointments;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Appointments
{
    [Authorize(Roles = "Admin,Staff,Doctor")]
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public AppointmentDetailDto Appointment { get; set; }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }
            return client;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = await client.GetAsync($"{baseUrl}/api/appointments/{id}");

            if (response.IsSuccessStatusCode)
            {
                Appointment = await response.Content.ReadFromJsonAsync<AppointmentDetailDto>();
                if (Appointment == null) return NotFound();
                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAcceptAsync(int id)
        {
            return await UpdateAppointmentStatus(id, "accept");
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            return await UpdateAppointmentStatus(id, "reject");
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            return await UpdateAppointmentStatus(id, "cancel");
        }

        private async Task<IActionResult> UpdateAppointmentStatus(int id, string action)
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = await client.PutAsync($"{baseUrl}/api/appointments/{id}/{action}", null);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage(new { id = id });
            }

            return RedirectToPage(new { id = id });
        }
    }
}