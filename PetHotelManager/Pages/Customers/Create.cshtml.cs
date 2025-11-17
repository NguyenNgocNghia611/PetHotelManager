using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Customer;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace PetHotelManager.Pages.Customers
{
    [Authorize(Roles = "Admin,Staff")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty] public CustomerCreateDto Input { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies["ApiToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var content = new StringContent(JsonSerializer.Serialize(Input), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/customermanagement/add", content);

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