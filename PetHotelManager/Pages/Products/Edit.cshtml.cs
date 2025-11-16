using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        public EditModel(IHttpClientFactory httpClientFactory) => _http = httpClientFactory;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public EditProductForm Form { get; set; } = new();

        public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var res = await client.GetAsync($"{baseUrl}/api/Products/{Id}");
                if (!res.IsSuccessStatusCode)
                {
                    Error = $"Không tải được sản phẩm. {(int)res.StatusCode} - {await res.Content.ReadAsStringAsync()}";
                    return Page();
                }

                var p = await res.Content.ReadFromJsonAsync<ProductDetail>();
                if (p == null)
                {
                    Error = "Không tìm thấy sản phẩm.";
                    return Page();
                }

                Form = new EditProductForm
                {
                    Id            = p.Id,
                    Name          = p.Name,
                    Price         = p.Price,
                    StockQuantity = p.StockQuantity,
                    Unit          = p.Unit,
                    IsActive      = p.IsActive
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

                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var dto = new
                {
                    Id            = Form.Id,
                    Name          = Form.Name,
                    Price         = Form.Price,
                    StockQuantity = Form.StockQuantity,
                    Unit          = Form.Unit,
                    IsActive      = Form.IsActive
                };

                var res = await client.PutAsJsonAsync($"{baseUrl}/api/Products/{Form.Id}", dto);
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Products/Index");
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

        public class ProductDetail
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string Unit { get; set; } = "";
            public bool IsActive { get; set; }
        }

        public class EditProductForm
        {
            public int Id { get; set; }

            [Required, StringLength(200)]
            public string Name { get; set; } = "";

            [Range(0, 100000000)]
            public decimal Price { get; set; }

            [Range(0, int.MaxValue)]
            public int StockQuantity { get; set; }

            [Required, StringLength(50)]
            public string Unit { get; set; } = "";

            public bool IsActive { get; set; }
        }
    }
}