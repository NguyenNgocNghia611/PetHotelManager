using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Data;
using PetHotelManager.Models;
using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Pages.Services
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
            public string Name { get;     set; }
            public string Category { get; set; }

            [Range(0, double.MaxValue, ErrorMessage = "Giá phải là một số không âm.")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "Đơn vị tính là bắt buộc.")]
            public string Unit { get; set; }
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

            var service = new Service
            {
                Name     = Input.Name,
                Category = Input.Category,
                Price    = Input.Price,
                Unit     = Input.Unit
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}