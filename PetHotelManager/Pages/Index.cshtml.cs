using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole("Admin") || User.IsInRole("Staff")))
            {
                return RedirectToPage("/Admin/Dashboard/Index");
            }

            return RedirectToPage("/Account/Login");
        }
    }
}