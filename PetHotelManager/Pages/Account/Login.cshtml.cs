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
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public LoginModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string? Error { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
            [Display(Name = "Tên đăng nhập")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var loginDto = new LoginDto
            {
                Username = Input.Username,
                Password = Input.Password
            };

            var client = _clientFactory.CreateClient("ApiClient");
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{baseUrl}/api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (loginResponse?.Token != null)
                    {
                        // Store JWT token in cookie to match existing pattern
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax,
                            Expires = Input.RememberMe 
                                ? DateTimeOffset.UtcNow.AddDays(30) 
                                : DateTimeOffset.UtcNow.AddHours(3)
                        };
                        
                        Response.Cookies.Append(".AspNetCore.Identity.Application", loginResponse.Token, cookieOptions);

                        // Determine redirect based on user roles
                        var roles = loginResponse.User?.Roles ?? new List<string>();
                        var isCustomer = roles.Contains("Customer");
                        var isHotel = roles.Contains("Admin") || roles.Contains("Staff") || roles.Contains("Veterinarian");

                        if (isCustomer)
                        {
                            return RedirectToPage("/Start");
                        }

                        if (isHotel && !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                        {
                            return LocalRedirect(ReturnUrl);
                        }

                        return RedirectToPage("/Start");
                    }
                }
                else
                {
                    // Parse error response
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (errorResponse != null && errorResponse.ContainsKey("message"))
                        {
                            Error = errorResponse["message"].ToString();
                        }
                        else
                        {
                            Error = "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";
                        }
                    }
                    catch
                    {
                        Error = "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";
                    }
                }
            }
            catch (Exception ex)
            {
                Error = $"Lỗi kết nối: {ex.Message}";
            }

            return Page();
        }
    }
}