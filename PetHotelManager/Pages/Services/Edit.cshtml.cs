using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Data;
using PetHotelManager.Models;
using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Pages.Services
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public int Id { get; set; }

            [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
            public string Name { get; set; }
            public string Category { get; set; }

            [Range(0, double.MaxValue, ErrorMessage = "Giá phải là một số không âm.")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "Đơn vị tính là bắt buộc.")]
            public string Unit { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = service.Id,
                Name = service.Name,
                Category = service.Category,
                Price = service.Price,
                Unit = service.Unit
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var serviceToUpdate = await _context.Services.FindAsync(Input.Id);
            if (serviceToUpdate == null)
            {
                return NotFound();
            }

            serviceToUpdate.Name = Input.Name;
            serviceToUpdate.Category = Input.Category;
            serviceToUpdate.Price = Input.Price;
            serviceToUpdate.Unit = Input.Unit;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}