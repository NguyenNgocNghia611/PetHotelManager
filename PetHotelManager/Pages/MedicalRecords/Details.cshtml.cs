using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.MedicalRecord;
using System.Net.Http;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.MedicalRecords
{
    [Authorize(Roles = "Admin,Staff,Veterinarian")]
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public MedicalRecordDto MedicalRecord { get; set; }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            
            // Get the JWT token from cookie and set as Bearer token
            var token = HttpContext.Request.Cookies["ApiToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            
            return client;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            try
            {
                MedicalRecord = await client.GetFromJsonAsync<MedicalRecordDto>($"{baseUrl}/api/medicalrecords/{id}");

                if (MedicalRecord == null)
                {
                    return NotFound();
                }

                return Page();
            }
            catch
            {
                return NotFound();
            }
        }
    }
}