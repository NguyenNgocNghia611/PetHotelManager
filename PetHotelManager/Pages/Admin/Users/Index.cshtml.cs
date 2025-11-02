using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Models;
using PetHotelManager.DTOs.Admin;

namespace PetHotelManager.Pages.Admin.Users
{
    using PetHotelManager.DTOs.Admin;

    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<UserManagementDto> UserList { get; set; } = new List<UserManagementDto>();

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                UserList.Add(new UserManagementDto
                {
                    Id          = user.Id,
                    UserName    = user.UserName,
                    FullName    = user.FullName,
                    Email       = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive    = user.IsActive,
                    Roles       = await _userManager.GetRolesAsync(user)
                });
            }
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToPage();
        }
    }
}