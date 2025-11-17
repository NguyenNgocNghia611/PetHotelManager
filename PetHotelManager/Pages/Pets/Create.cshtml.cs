using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetHotelManager.DTOs.Pets;
using PetHotelManager.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Pets
{
    [Authorize(Roles = "Admin,Staff,Veterinarian")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList Customers { get; set; }

        public class InputModel : CreatePetDto
        {
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies["ApiToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task OnGetAsync()
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var customers = await client.GetFromJsonAsync<List<ApplicationUser>>($"{baseUrl}/api/customermanagement/list") ?? new List<ApplicationUser>();
            Customers = new SelectList(customers, "Id", "FullName");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(Input.UserId), nameof(Input.UserId));
            content.Add(new StringContent(Input.Name), nameof(Input.Name));
            content.Add(new StringContent(Input.Species ?? ""), nameof(Input.Species));
            content.Add(new StringContent(Input.Breed ?? ""), nameof(Input.Breed));
            content.Add(new StringContent(Input.Age.ToString()), nameof(Input.Age));
            content.Add(new StringContent(Input.Color ?? ""), nameof(Input.Color));
            content.Add(new StringContent(Input.HealthStatus ?? ""), nameof(Input.HealthStatus));

            if (Input.ImageFile != null)
            {
                var fileContent = new StreamContent(Input.ImageFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(Input.ImageFile.ContentType);
                content.Add(fileContent, nameof(Input.ImageFile), Input.ImageFile.FileName);
            }

            var response = await client.PostAsync($"{baseUrl}/api/pets", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./Index");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Lỗi từ API: {errorContent}");
            await OnGetAsync();
            return Page();
        }
    }
}