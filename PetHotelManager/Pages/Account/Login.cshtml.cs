using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Models;

namespace PetHotelManager.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string? Error { get; set; }

        public class InputModel
        {
            [Required]
            public string UserNameOrEmail { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Tra cứu user bằng email hoặc username
            var user = await _userManager.FindByEmailAsync(Input.UserNameOrEmail)
                       ?? await _userManager.FindByNameAsync(Input.UserNameOrEmail);

            if (user == null)
            {
                Error = "Tài khoản không tồn tại.";
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                Error = "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";
                return Page();
            }

            // Lấy role trực tiếp từ userManager (Claims chưa update trong request hiện tại)
            var roles = await _userManager.GetRolesAsync(user);
            var isCustomer = roles.Contains("Customer");
            var isHotel = roles.Contains("Admin") || roles.Contains("Staff") || roles.Contains("Veterinarian") || roles.Contains("Doctor");

            // Chiến lược điều hướng:
            // - Customer: luôn về /Start để tránh ReturnUrl dính trang hotel
            // - Hotel: nếu ReturnUrl là LocalUrl thì cho phép, ngược lại về /Start
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
}