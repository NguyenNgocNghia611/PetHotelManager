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
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public CusUpdateDto Input { get; set; } = new CusUpdateDto();

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; }

        public string Username { get; set; }

        private async Task<HttpClient> GetAuthenticatedClientAsync()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }
            return client;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null) return NotFound();
            Id = id;

            var client = await GetAuthenticatedClientAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var historyTask = client.GetAsync($"{baseUrl}/api/customermanagement/{id}/history");
            var listTask = client.GetAsync($"{baseUrl}/api/customermanagement/list");

            await Task.WhenAll(historyTask, listTask);

            var historyResponse = await historyTask;
            var listResponse = await listTask;

            if (historyResponse.IsSuccessStatusCode && listResponse.IsSuccessStatusCode)
            {
                var historyStream = await historyResponse.Content.ReadAsStreamAsync();
                var history = await JsonSerializer.DeserializeAsync<CustomerHistoryResponse>(historyStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (history?.Customer == null) return NotFound();

                var listStream = await listResponse.Content.ReadAsStreamAsync();
                var customerList = await JsonSerializer.DeserializeAsync<List<UserFromList>>(listStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var currentUserFromList = customerList?.FirstOrDefault(c => c.Id == id);

                Input = new CusUpdateDto
                {
                    FullName = history.Customer.FullName,
                    Email = history.Customer.Email,
                    PhoneNumber = history.Customer.PhoneNumber,
                    IsActive = currentUserFromList?.IsActive ?? true
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

            var client = await GetAuthenticatedClientAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var content = new StringContent(JsonSerializer.Serialize(Input), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{baseUrl}/api/customermanagement/{Id}/update", content);

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

        private class CustomerHistoryResponse
        {
            public CustomerInfo Customer { get; set; }
        }
        private class CustomerInfo
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }

        private class UserFromList
        {
            public string Id { get; set; }
            public bool IsActive { get; set; }
        }
    }
}