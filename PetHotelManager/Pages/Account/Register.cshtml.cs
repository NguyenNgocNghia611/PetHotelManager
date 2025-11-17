using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
            [Display(Name = "Tên đăng nhập")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Họ và tên là bắt buộc")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 8)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var client = _clientFactory.CreateClient("ApiClient");
                var baseUrl = $"{Request.Scheme}://{Request.Host}";

                // Create the registration payload
                var registerDto = new
                {
                    username = Input.Username,
                    email = Input.Email,
                    password = Input.Password,
                    fullName = Input.FullName
                };

                var json = JsonSerializer.Serialize(registerDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Call the register API
                var response = await client.PostAsync($"{baseUrl}/api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    // Registration successful, redirect to login
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToPage("/Account/Login");
                }
                else
                {
                    // Registration failed, show error
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    try
                    {
                        // Try to parse the error response
                        var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        if (errorResponse.TryGetProperty("Message", out var message))
                        {
                            ErrorMessage = message.GetString();
                        }
                        else if (errorResponse.TryGetProperty("message", out var msg))
                        {
                            ErrorMessage = msg.GetString();
                        }
                        else
                        {
                            ErrorMessage = "Đăng ký thất bại. Vui lòng thử lại.";
                        }
                    }
                    catch
                    {
                        ErrorMessage = errorContent;
                    }

                    ModelState.AddModelError(string.Empty, ErrorMessage ?? "Đăng ký thất bại");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Có lỗi xảy ra: {ex.Message}";
                ModelState.AddModelError(string.Empty, ErrorMessage);
                return Page();
            }
        }
    }
}
