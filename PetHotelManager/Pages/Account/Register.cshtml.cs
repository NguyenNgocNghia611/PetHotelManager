using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Auth;

namespace PetHotelManager.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public RegisterModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
            [Display(Name = "Tên đăng nhập")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email là bắt buộc.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var registerDto = new RegisterDto
            {
                Username = Input.Username,
                Email = Input.Email,
                Password = Input.Password,
                FullName = Input.FullName
            };

            var client = _clientFactory.CreateClient("ApiClient");
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{baseUrl}/api/customer/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToPage("/Account/Login");
                }
                else
                {
                    // Parse error response
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                        if (errorResponse != null && errorResponse.ContainsKey("Message"))
                        {
                            ModelState.AddModelError(string.Empty, errorResponse["Message"].ToString() ?? "Đăng ký thất bại.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối: {ex.Message}");
            }

            return Page();
        }
    }
}
