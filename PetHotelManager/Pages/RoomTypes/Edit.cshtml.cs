using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages.RoomTypes
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        public EditModel(IHttpClientFactory httpClientFactory) => _http = httpClientFactory;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public EditRoomTypeForm Form { get; set; } = new();

        public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var client  = _http.CreateClient();

                var cookieHeader = Request.Headers.Cookie.ToString();
                if (!string.IsNullOrEmpty(cookieHeader))
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var res = await client.GetAsync($"{baseUrl}/api/RoomTypes/{Id}");
                if (!res.IsSuccessStatusCode)
                {
                    Error = $"Không tải được loại phòng. {(int)res.StatusCode} - {await res.Content.ReadAsStringAsync()}";
                    return Page();
                }

                var dto = await res.Content.ReadFromJsonAsync<RoomTypeDetail>();
                if (dto == null)
                {
                    Error = "Không tìm thấy loại phòng.";
                    return Page();
                }

                Form = new EditRoomTypeForm
                {
                    Id          = dto.Id,
                    TypeName    = dto.TypeName,
                    PricePerDay = dto.PricePerDay,
                    Description = dto.Description
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
                    Id          = Form.Id,
                    TypeName    = Form.TypeName,
                    PricePerDay = Form.PricePerDay,
                    Description = Form.Description
                };

                var res = await client.PutAsJsonAsync($"{baseUrl}/api/RoomTypes/{Form.Id}", dto);
                if (res.IsSuccessStatusCode)
                {
                    return RedirectToPage("/RoomTypes/Index");
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
            return Page();
        }

        public class RoomTypeDetail
        {
            public int Id { get; set; }
            public string TypeName { get; set; } = "";
            public decimal PricePerDay { get; set; }
            public string? Description { get; set; }
        }

        public class EditRoomTypeForm
        {
            public int Id { get; set; }

            [Required, StringLength(100)]
            public string TypeName { get; set; } = "";

            [Range(0, 100000000)]
            public decimal PricePerDay { get; set; }

            [StringLength(500)]
            public string? Description { get; set; }
        }
    }
}