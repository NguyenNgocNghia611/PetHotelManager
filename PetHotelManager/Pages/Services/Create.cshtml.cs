using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Services
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory;
        }

        [BindProperty] public CreateServiceForm Form { get; set; } = new();

        public string? Error { get; set; }
        public string? Success { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                // Forward Identity cookie để API nhận diện Admin
                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var dto = new
                {
                    Name     = Form.Name,
                    Category = Form.Category,
                    Price    = Form.Price,
                    Unit     = Form.Unit
                };

                var res = await client.PostAsJsonAsync($"{baseUrl}/api/Services", dto);
                if (res.IsSuccessStatusCode)
                {
                    // Thành công -> quay lại danh sách
                    return RedirectToPage("/Services/Index");
                }
                else
                {
                    // Ghi lỗi để hiển thị trên trang
                    var body = await res.Content.ReadAsStringAsync();
                    Error = $"API /api/Services trả về {(int)res.StatusCode}: {body}";
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }

        public class CreateServiceForm
        {
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