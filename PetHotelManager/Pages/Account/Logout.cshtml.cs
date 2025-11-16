using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using PetHotelManager.Models;

namespace PetHotelManager.Pages.Account
{
    using Microsoft.AspNetCore.Authentication;

    [Authorize]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public void OnGet()
        {
            // Nếu muốn: hiển thị xác nhận
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();

            // Xóa thêm (thường không cần, bổ sung để chắc chắn)
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // Nếu bạn có JWT lưu ở localStorage trong SPA khác thì cần tự xóa phía client.
            return RedirectToPage("/Index");
        }
    }
}