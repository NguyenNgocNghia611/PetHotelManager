using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Admin;
using System.Text;
using System.Text.Json;

namespace PetHotelManager.Pages.Admin.Users
{
    using PetHotelManager.DTOs.Admin;

    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty] public InputModel Input { get; set; }

        public List<string> Roles { get; set; } = new List<string> { "Staff", "Doctor", "Admin" };

        public class InputModel
        {
            public string  Id          { get; set; }
            public string  Username    { get; set; }
            public string  FullName    { get; set; }
            public string  Email       { get; set; }
            public string? PhoneNumber { get; set; }
            public string  Role        { get; set; }
            public bool    IsActive    { get; set; }
        }

        private async Task<HttpClient> GetAuthenticatedClientAsync()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token  = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }
            return client;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null) return NotFound();

            var client   = await GetAuthenticatedClientAsync();
            var baseUrl  = $"{Request.Scheme}://{Request.Host}";
            var response = await client.GetAsync($"{baseUrl}/api/admin/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var stream  = await response.Content.ReadAsStreamAsync();
                var userDto = await JsonSerializer.DeserializeAsync<UserManagementDto>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (userDto == null) return NotFound();

                Input = new InputModel
                {
                    Id          = userDto.Id,
                    Username    = userDto.UserName,
                    FullName    = userDto.FullName,
                    Email       = userDto.Email,
                    PhoneNumber = userDto.PhoneNumber,
                    IsActive    = userDto.IsActive,
                    Role        = userDto.Roles.FirstOrDefault() ?? string.Empty
                };
                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var updateUserDto = new AdminUpdateUserDto
            {
                FullName    = Input.FullName,
                Email       = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                Role        = Input.Role
            };

            var client  = await GetAuthenticatedClientAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var content = new StringContent(JsonSerializer.Serialize(updateUserDto), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{baseUrl}/api/admin/users/{Input.Id}", content);

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