using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.MedicalRecord;
using PetHotelManager.DTOs.Pets;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Veterinarian.MedicalRecords
{
    [Authorize(Roles = "Veterinarian")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<PetListDto> Pets { get; set; } = new();
        public List<MedicalRecordDto> MedicalRecords { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public int? SelectedPetId { get; set; }

        public string? ErrorMessage { get; set; }

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

        public async Task OnGetAsync()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                // Load pets list
                Pets = await client.GetFromJsonAsync<List<PetListDto>>($"{baseUrl}/api/pets") 
                    ?? new List<PetListDto>();

                // If a pet is selected, load its medical records
                if (SelectedPetId.HasValue)
                {
                    MedicalRecords = await client.GetFromJsonAsync<List<MedicalRecordDto>>(
                        $"{baseUrl}/api/medicalrecords/pet/{SelectedPetId.Value}") 
                        ?? new List<MedicalRecordDto>();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
            }
        }
    }
}
