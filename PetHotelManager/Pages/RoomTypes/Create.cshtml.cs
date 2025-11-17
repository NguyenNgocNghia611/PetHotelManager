using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.RoomTypes
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory;
        }

        [BindProperty] public CreateRoomTypeForm Form { get; set; } = new();

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
                    TypeName    = Form.TypeName,
                    PricePerDay = Form.PricePerDay,
                    Description = Form.Description
                };

                var res = await client.PostAsJsonAsync($"{baseUrl}/api/RoomTypes", dto);
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToPage("/RoomTypes/Index");
                }
                else
                {
                    Error = $"API /api/RoomTypes trả về {(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }

        public class CreateRoomTypeForm
        {
            [Required, StringLength(100)]
            public string TypeName { get; set; } = "";

            [Range(0, 100000000)]
            public decimal PricePerDay { get; set; }

            [StringLength(500)]
            public string? Description { get; set; }
        }
    }
}