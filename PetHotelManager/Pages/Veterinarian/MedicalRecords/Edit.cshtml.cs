using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.MedicalRecord;

namespace PetHotelManager.Pages.Veterinarian.MedicalRecords
{
    [Authorize(Roles = "Veterinarian")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public MedicalRecordDto? Record { get; set; }
        public string? ErrorMessage { get; set; }
        public bool EditNotSupported { get; set; } = true;

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
            try
            {
                var client = GetAuthenticatedClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                var response = await client.GetAsync($"{baseUrl}/api/medicalrecords/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    Record = await response.Content.ReadFromJsonAsync<MedicalRecordDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else
                {
                    ErrorMessage = $"Lỗi khi tải hồ sơ: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
            }

            return Page();
        }
    }
}
