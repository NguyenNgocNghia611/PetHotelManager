using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.RoomTypes;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.RoomTypes
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public UpdateRoomTypeDto Input { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var dto = await client.GetFromJsonAsync<RoomTypeDto>($"{baseUrl}/api/roomtypes/{id}");

            if (dto == null) return NotFound();

            Input = new UpdateRoomTypeDto
            {
                Id = dto.Id,
                TypeName = dto.TypeName,
                Description = dto.Description,
                PricePerDay = dto.PricePerDay
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = await client.PutAsJsonAsync($"{baseUrl}/api/roomtypes/{Input.Id}", Input);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật loại phòng từ API.");
            return Page();
        }
    }
}