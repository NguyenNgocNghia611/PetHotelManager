using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PetHotelManager.Pages.Veterinarian.MedicalRecords
{
    [Authorize(Roles = "Veterinarian")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory;
        }

        [BindProperty] public CreateMedicalRecordForm Form { get; set; } = new();

        public List<PetItem> Pets { get; set; } = new();
        public List<ProductItem> Products { get; set; } = new();

        public string? Error { get; set; }
        public string? Success { get; set; }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _http.CreateClient("ApiClient");
            
            // Get the JWT token from cookie and set as Bearer token
            var token = HttpContext.Request.Cookies["ApiToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            
            return client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddPrescriptionAsync()
        {
            // Add empty prescription row
            Form.Prescriptions.Add(new PrescriptionRow());
            await LoadLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRemovePrescriptionAsync(int index)
        {
            if (index >= 0 && index < Form.Prescriptions.Count)
                Form.Prescriptions.RemoveAt(index);

            await LoadLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync();
                return Page();
            }

            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client = GetAuthenticatedClient();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    Error = "Không xác định được người dùng hiện tại.";
                    await LoadLookupsAsync();
                    return Page();
                }

                var dto = new CreateMedicalRecordDto
                {
                    PetId = Form.PetId,
                    ExaminationDate = Form.ExaminationDate,
                    Symptoms = Form.Symptoms,
                    Diagnosis = Form.Diagnosis,
                    Prescriptions = Form.Prescriptions
                        .Where(p => p.ProductId.HasValue && p.Quantity > 0)
                        .Select(p => new PrescriptionDto
                        {
                            ProductId = p.ProductId!.Value,
                            Quantity = p.Quantity,
                            Dosage = p.Dosage
                        }).ToList()
                };

                var res = await client.PostAsJsonAsync($"{baseUrl}/api/medicalrecords", dto);
                
                if (res.IsSuccessStatusCode)
                {
                    Success = "Tạo hồ sơ khám thành công!";
                    // Reset form
                    Form = new CreateMedicalRecordForm();
                    ModelState.Clear();
                }
                else
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    Error = $"Lỗi ({res.StatusCode}): {errorContent}";
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi: {ex.Message}";
            }

            await LoadLookupsAsync();
            return Page();
        }

        private async Task LoadLookupsAsync()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var client = GetAuthenticatedClient();

            try
            {
                // Load pets
                Pets = await client.GetFromJsonAsync<List<PetItem>>($"{baseUrl}/api/pets") 
                    ?? new List<PetItem>();

                // Load products
                var productsFull = await client.GetFromJsonAsync<List<ProductItem>>($"{baseUrl}/api/products") 
                    ?? new List<ProductItem>();
                Products = productsFull.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
            }
            catch (Exception ex)
            {
                Error = $"Lỗi khi tải dữ liệu: {ex.Message}";
            }
        }

        // Form & DTO internal classes
        public class CreateMedicalRecordForm
        {
            [Required(ErrorMessage = "Vui lòng chọn thú cưng")]
            public int PetId { get; set; }
            
            [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
            public DateTime ExaminationDate { get; set; } = DateTime.Now;
            
            public string? Symptoms { get; set; }
            public string? Diagnosis { get; set; }
            public List<PrescriptionRow> Prescriptions { get; set; } = new();
        }

        public class PrescriptionRow
        {
            public int? ProductId { get; set; }
            
            [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải >= 0")]
            public int Quantity { get; set; }
            
            public string? Dosage { get; set; }
        }

        public class PetItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Species { get; set; } = "";
            public string? OwnerName { get; set; }
        }

        public class ProductItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string Unit { get; set; } = "";
            public bool IsActive { get; set; }
        }

        public class CreateMedicalRecordDto
        {
            public int PetId { get; set; }
            public DateTime ExaminationDate { get; set; }
            public string? Symptoms { get; set; }
            public string? Diagnosis { get; set; }
            public List<PrescriptionDto>? Prescriptions { get; set; }
        }

        public class PrescriptionDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public string? Dosage { get; set; }
        }
    }
}
