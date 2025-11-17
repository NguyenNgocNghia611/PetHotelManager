using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Pets;

namespace PetHotelManager.Pages.Veterinarian.Pets
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
        public List<PetListDto> FilteredPets { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public string? ErrorMessage { get; set; }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
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

                // Load all pets
                Pets = await client.GetFromJsonAsync<List<PetListDto>>($"{baseUrl}/api/pets") 
                    ?? new List<PetListDto>();

                // Filter by search term if provided
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    var searchLower = SearchTerm.Trim().ToLower();
                    FilteredPets = Pets.Where(p =>
                        (p.Name != null && p.Name.ToLower().Contains(searchLower)) ||
                        (p.Species != null && p.Species.ToLower().Contains(searchLower)) ||
                        (p.OwnerName != null && p.OwnerName.ToLower().Contains(searchLower))
                    ).ToList();
                }
                else
                {
                    FilteredPets = Pets;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
            }
        }
    }
}
