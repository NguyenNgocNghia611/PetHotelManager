using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetHotelManager.DTOs.Rooms;
using PetHotelManager.DTOs.RoomTypes;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Rooms
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
        public UpdateRoomDto Input { get; set; }

        public SelectList RoomTypes { get; set; }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }
            return client;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var roomTask = client.GetFromJsonAsync<RoomDto>($"{baseUrl}/api/rooms/{id}");
            var roomTypesTask = client.GetFromJsonAsync<List<RoomTypeDto>>($"{baseUrl}/api/roomtypes");

            await Task.WhenAll(roomTask, roomTypesTask);

            var roomDto = await roomTask;
            if (roomDto == null) return NotFound();

            var roomTypes = (await roomTypesTask) ?? new List<RoomTypeDto>();
            RoomTypes = new SelectList(roomTypes, "Id", "TypeName");

            var selectedRoomType = roomTypes.FirstOrDefault(rt => rt.TypeName == roomDto.RoomTypeName);

            Input = new UpdateRoomDto
            {
                Id = roomDto.Id,
                RoomNumber = roomDto.RoomNumber,
                Status = roomDto.Status,
                RoomTypeId = selectedRoomType?.Id ?? 0
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var client = GetAuthenticatedClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var roomTypes = await client.GetFromJsonAsync<List<RoomTypeDto>>($"{baseUrl}/api/roomtypes") ?? new List<RoomTypeDto>();
                RoomTypes = new SelectList(roomTypes, "Id", "TypeName");
                return Page();
            }

            var apiClient = GetAuthenticatedClient();
            var apiBaseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = await apiClient.PutAsJsonAsync($"{apiBaseUrl}/api/rooms/{Input.Id}", Input);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật phòng từ API.");
            await OnGetAsync(Input.Id);
            return Page();
        }
    }
}