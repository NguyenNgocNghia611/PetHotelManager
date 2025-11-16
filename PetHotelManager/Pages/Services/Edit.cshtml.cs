using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Services
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        public EditModel(IHttpClientFactory httpClientFactory) => _http = httpClientFactory;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public EditServiceForm Form { get; set; } = new();

        public string? Error { get; set; }
        public string? Success { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                // GET có thể public, nhưng forward cookie cũng không hại
                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var res = await client.GetAsync($"{baseUrl}/api/Services/{Id}");
                if (!res.IsSuccessStatusCode)
                {
                    Error = $"Không tải được dịch vụ. {(int)res.StatusCode} - {await res.Content.ReadAsStringAsync()}";
                    return Page();
                }

                var svc = await res.Content.ReadFromJsonAsync<ServiceDetail>();
                if (svc == null)
                {
                    Error = "Không tìm thấy dịch vụ.";
                    return Page();
                }

                Form = new EditServiceForm
                {
                    Id       = svc.Id,
                    Name     = svc.Name,
                    Category = svc.Category,
                    Price    = svc.Price,
                    Unit     = svc.Unit
                };
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                // Forward cookie trước khi PUT
                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var dto = new
                {
                    Id       = Form.Id,
                    Name     = Form.Name,
                    Category = Form.Category,
                    Price    = Form.Price,
                    Unit     = Form.Unit
                };

                var res = await client.PutAsJsonAsync($"{baseUrl}/api/Services/{Form.Id}", dto);
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Services/Index");
                }
                else
                {
                    Error = $"Cập nhật thất bại: {(int)res.StatusCode} - {await res.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }

        public class ServiceDetail
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Category { get; set; } = "";
            public decimal Price { get; set; }
            public string Unit { get; set; } = "";
        }

        public class EditServiceForm
        {
            public int Id { get; set; }

            [Required, StringLength(200)]
            public string Name { get; set; } = "";

            [Required, StringLength(100)]
            public string Category { get; set; } = "";

            [Range(0, 100000000)]
            public decimal Price { get; set; }

            [Required, StringLength(50)]
            public string Unit { get; set; } = "";
        }
    }
}