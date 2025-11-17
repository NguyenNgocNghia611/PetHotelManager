using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.MedicalRecord;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.MedicalRecords
{
    [Authorize(Roles = "Admin,Staff,Veterinarian")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<MedicalRecordDto> MedicalRecords { get; set; } = new List<MedicalRecordDto>();
        public List<MedicalRecordDto> AllMedicalRecords { get; set; } = new List<MedicalRecordDto>();

        [BindProperty(SupportsGet = true)]
        public int? PetId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

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

        public async Task OnGetAsync()
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // Lấy danh sách tất cả pets
            var pets = await client.GetFromJsonAsync<List<PetDto>>($"{baseUrl}/api/pets") ?? new List<PetDto>();

            // Lấy hồ sơ khám của tất cả pets
            foreach (var pet in pets)
            {
                var records = await client.GetFromJsonAsync<List<MedicalRecordDto>>($"{baseUrl}/api/medicalrecords/pet/{pet.Id}");
                if (records != null && records.Any())
                {
                    AllMedicalRecords.AddRange(records);
                }
            }

            // Sắp xếp theo ngày khám mới nhất
            AllMedicalRecords = AllMedicalRecords.OrderByDescending(m => m.ExaminationDate).ToList();

            // Lọc theo PetId nếu có
            if (PetId.HasValue)
            {
                MedicalRecords = AllMedicalRecords.Where(m => m.PetId == PetId.Value).ToList();
            }
            // Lọc theo SearchTerm nếu có
            else if (!string.IsNullOrEmpty(SearchTerm))
            {
                MedicalRecords = AllMedicalRecords.Where(m =>
                    m.VeterinarianName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (m.Symptoms != null && m.Symptoms.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Diagnosis != null && m.Diagnosis.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            else
            {
                // Hiển thị tất cả
                MedicalRecords = AllMedicalRecords;
            }
        }

        public class PetDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}