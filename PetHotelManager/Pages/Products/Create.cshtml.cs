using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory;
        }

        [BindProperty] public CreateProductForm Form { get; set; } = new();

        public string? Error { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var dto = new
                {
                    Name          = Form.Name,
                    Price         = Form.Price,
                    StockQuantity = Form.StockQuantity,
                    Unit          = Form.Unit,
                    IsActive      = Form.IsActive
                };

                var res = await client.PostAsJsonAsync($"{baseUrl}/api/Products", dto);
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Products/Index");
                }
                else
                {
                    var body = await res.Content.ReadAsStringAsync();
                    Error = $"API /api/Products trả về {(int)res.StatusCode}: {body}";
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }

        public class CreateProductForm
        {
            [Required, StringLength(200)]
            public string Name { get; set; } = "";

            [Range(0, 100000000)]
            public decimal Price { get; set; }

            [Range(0, int.MaxValue)]
            public int StockQuantity { get; set; }

            [Required, StringLength(50)]
            public string Unit { get; set; } = "";

            public bool IsActive { get; set; } = true;
        }
    }
}