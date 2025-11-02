using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Models;
using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Pages.Customers
{
    [Authorize(Roles = "Admin,Staff")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
            [DataType(DataType.Password)]
            [StringLength(100, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.", MinimumLength = 8)]
            public string Password { get; set; }

            [Required(ErrorMessage = "Họ và tên là bắt buộc")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Số điện thoại")]
            public string? PhoneNumber { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userExists = await _userManager.FindByNameAsync(Input.Username);
            if (userExists != null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập đã tồn tại.");
                return Page();
            }

            var newCustomer = new ApplicationUser
            {
                UserName = Input.Username,
                Email = Input.Email,
                FullName = Input.FullName,
                PhoneNumber = Input.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newCustomer, Input.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }

                await _userManager.AddToRoleAsync(newCustomer, "Customer");

                return RedirectToPage("./Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}