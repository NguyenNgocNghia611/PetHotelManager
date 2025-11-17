using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Rooms
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory;
        }

        [BindProperty] public CreateRoomForm Form { get; set; } = new();
        public List<RoomTypeItem> RoomTypes { get; set; } = new();

        public string? Error { get; set; }

        public async Task OnGet()
        {
            await LoadRoomTypesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRoomTypesAsync();
                return Page();
            }

            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var dto = new
                {
                    RoomNumber = Form.RoomNumber,
                    RoomTypeId = Form.RoomTypeId,
                    Status     = Form.Status
                };

                var res = await client.PostAsJsonAsync($"{baseUrl}/api/Rooms", dto);
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Rooms/Index");
                }
                else
                {
                    Error = $"API /api/Rooms trả về {(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}";
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            await LoadRoomTypesAsync();
            return Page();
        }

        private async Task LoadRoomTypesAsync()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var client  = _http.CreateClient();
            RoomTypes   = await client.GetFromJsonAsync<List<RoomTypeItem>>($"{baseUrl}/api/RoomTypes") ?? new();
        }

        public class CreateRoomForm
        {
            [Required, StringLength(50)]
            public string RoomNumber { get; set; } = "";

            [Required]
            public int RoomTypeId { get; set; }

            [Required, StringLength(50)]
            public string Status { get; set; } = "Available";
        }

        public class RoomTypeItem
        {
            public int Id { get; set; }
            public string TypeName { get; set; } = "";
            public decimal PricePerDay { get; set; }
        }
    }
}