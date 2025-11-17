using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Models;

namespace PetHotelManager.Pages.Customer
{
    [Authorize(Roles = "Customer")]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileModel(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public ProfileInputModel ProfileInput { get; set; } = new();

        [BindProperty]
        public ChangePasswordInputModel PasswordInput { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            ProfileInput = new ProfileInputModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? ""
            };

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = ProfileInput.FullName;
            user.PhoneNumber = ProfileInput.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                SuccessMessage = "Cập nhật thông tin thành công.";
            }
            else
            {
                ErrorMessage = "Có lỗi xảy ra khi cập nhật thông tin.";
            }

            // Reload profile data
            ProfileInput.Email = user.Email ?? "";
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            // Clear profile validation errors
            ModelState.Remove("ProfileInput.FullName");
            ModelState.Remove("ProfileInput.Email");
            ModelState.Remove("ProfileInput.PhoneNumber");

            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ProfileInput = new ProfileInputModel
                    {
                        FullName = user.FullName,
                        Email = user.Email ?? "",
                        PhoneNumber = user.PhoneNumber ?? ""
                    };
                }
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                currentUser,
                PasswordInput.CurrentPassword,
                PasswordInput.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(currentUser);
                SuccessMessage = "Đổi mật khẩu thành công.";
                PasswordInput = new ChangePasswordInputModel(); // Clear password fields
            }
            else
            {
                ErrorMessage = "Đổi mật khẩu thất bại. ";
                foreach (var error in result.Errors)
                {
                    ErrorMessage += error.Description + " ";
                }
            }

            // Reload profile data
            ProfileInput = new ProfileInputModel
            {
                FullName = currentUser.FullName,
                Email = currentUser.Email ?? "",
                PhoneNumber = currentUser.PhoneNumber ?? ""
            };

            return Page();
        }

        public class ProfileInputModel
        {
            [Required(ErrorMessage = "Họ tên không được để trống")]
            [StringLength(100)]
            [Display(Name = "Họ tên")]
            public string FullName { get; set; } = "";

            [Display(Name = "Email")]
            public string Email { get; set; } = "";

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string? PhoneNumber { get; set; }
        }

        public class ChangePasswordInputModel
        {
            [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu hiện tại")]
            public string CurrentPassword { get; set; } = "";

            [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu mới")]
            public string NewPassword { get; set; } = "";

            [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            [Display(Name = "Xác nhận mật khẩu mới")]
            public string ConfirmPassword { get; set; } = "";
        }
    }
}
