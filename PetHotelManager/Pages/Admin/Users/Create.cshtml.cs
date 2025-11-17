using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Admin;
using System.Text;
using System.Text.Json;

namespace PetHotelManager.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty] public AdminCreateUserDto Input { get; set; }

        public List<string> Roles { get; set; } = new List<string> { "Staff", "Veterinarian" };

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _clientFactory.CreateClient("ApiClient");
            var token  = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var content = new StringContent(JsonSerializer.Serialize(Input), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/admin/users", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Lỗi từ API: {errorContent}");
                return Page();
            }
        }
    }
}