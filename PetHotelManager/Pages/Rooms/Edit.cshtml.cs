using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.Rooms
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        public EditModel(IHttpClientFactory httpClientFactory) => _http = httpClientFactory;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty] public EditRoomForm Form { get; set; } = new();
        public List<RoomTypeItem> RoomTypes { get; set; } = new();

        public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                // forward cookie (dù GET thường public)
                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                // Load detail
                var res = await client.GetAsync($"{baseUrl}/api/Rooms/{Id}");
                if (!res.IsSuccessStatusCode)
                {
                    Error = $"Không tải được phòng. {(int)res.StatusCode} - {await res.Content.ReadAsStringAsync()}";
                    return Page();
                }
                var dto = await res.Content.ReadFromJsonAsync<RoomDetail>();
                if (dto == null)
                {
                    Error = "Không tìm thấy phòng.";
                    return Page();
                }

                // Load room types
                RoomTypes = await client.GetFromJsonAsync<List<RoomTypeItem>>($"{baseUrl}/api/RoomTypes") ?? new();

                // Map RoomTypeId từ tên (nếu API không trả RoomTypeId)
                var currentTypeId = RoomTypes.FirstOrDefault(t => t.TypeName == dto.RoomTypeName)?.Id ?? 0;

                Form = new EditRoomForm
                {
                    Id         = dto.Id,
                    RoomNumber = dto.RoomNumber,
                    RoomTypeId = currentTypeId,
                    Status     = dto.Status
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
            {
                await ReloadRoomTypesForPostAsync();
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
                    Id         = Form.Id,
                    RoomNumber = Form.RoomNumber,
                    RoomTypeId = Form.RoomTypeId,
                    Status     = Form.Status
                };

                var res = await client.PutAsJsonAsync($"{baseUrl}/api/Rooms/{Form.Id}", dto);
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Rooms/Index");
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

            await ReloadRoomTypesForPostAsync();
            return Page();
        }

        private async Task ReloadRoomTypesForPostAsync()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var client  = _http.CreateClient();
            RoomTypes   = await client.GetFromJsonAsync<List<RoomTypeItem>>($"{baseUrl}/api/RoomTypes") ?? new();
        }

        public class EditRoomForm
        {
            public int Id { get; set; }

            [Required, StringLength(50)]
            public string RoomNumber { get; set; } = "";

            [Required]
            public int RoomTypeId { get; set; }

            [Required, StringLength(50)]
            public string Status { get; set; } = "Available";
        }

        public class RoomDetail
        {
            public int Id { get; set; }
            public string RoomNumber { get; set; } = "";
            public string RoomTypeName { get; set; } = "";
            public decimal PricePerDay { get; set; }
            public string Status { get; set; } = "";
        }

        public class RoomTypeItem
        {
            public int Id { get; set; }
            public string TypeName { get; set; } = "";
            public decimal PricePerDay { get; set; }
        }
    }
}