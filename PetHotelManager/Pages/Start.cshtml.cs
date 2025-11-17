using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetHotelManager.Pages
{
    [Authorize]
    public class StartModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Điều hướng mặc định theo vai trò
            if (User.IsInRole("Admin"))
                return Redirect("/Services"); // hoặc /Admin/Dashboard nếu có

            if (User.IsInRole("Staff") || User.IsInRole("Veterinarian") || User.IsInRole("Doctor"))
                return Redirect("/Appointments/Filter");

            if (User.IsInRole("Customer"))
                return Redirect("/MyPets"); // hoặc "/Appointments/Create" nếu bạn muốn ưu tiên đặt lịch

            // Nếu không khớp role nào (trường hợp lạ): về trang chủ
            return RedirectToPage("/Index");
        }
    }
}