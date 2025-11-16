using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PetHotelManager.Pages.MedicalRecords
{
    [Authorize(Roles = "Staff,Doctor,Veterinarian")]
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

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddPrescriptionAsync()
        {
            // Thêm hàng trống prescription
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
                var client = _http.CreateClient();

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

                var res = await client.PostAsJsonAsync($"{baseUrl}/api/MedicalRecords", dto);
                if (res.IsSuccessStatusCode)
                {
                    Success = "Tạo hồ sơ khám thành công!";
                    // Reset form
                    Form = new CreateMedicalRecordForm();
                }
                else
                {
                    Error = await res.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            await LoadLookupsAsync();
            return Page();
        }

        private async Task LoadLookupsAsync()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var client = _http.CreateClient();

            // Pets (Staff, Vet có quyền)
            Pets = await client.GetFromJsonAsync<List<PetItem>>($"{baseUrl}/api/Pets") ?? new List<PetItem>();

            // Products (Admin,Staff,Veterinarian) - Staff/Vet có thể xem
            var productsFull = await client.GetFromJsonAsync<List<ProductItem>>($"{baseUrl}/api/Products") ?? new List<ProductItem>();
            Products = productsFull.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
        }

        // Form & DTO internal classes
        public class CreateMedicalRecordForm
        {
            [Required] public int PetId { get; set; }
            [Required] public DateTime ExaminationDate { get; set; } = DateTime.Now;
            public string? Symptoms { get; set; }
            public string? Diagnosis { get; set; }
            public List<PrescriptionRow> Prescriptions { get; set; } = new();
        }

        public class PrescriptionRow
        {
            public int? ProductId { get; set; }
            public int Quantity { get; set; }
            public string? Dosage { get; set; }
        }

        public class PetItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Species { get; set; } = "";
            public string? Breed { get; set; }
            public string? HealthStatus { get; set; }
            public string? UserId { get; set; }
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