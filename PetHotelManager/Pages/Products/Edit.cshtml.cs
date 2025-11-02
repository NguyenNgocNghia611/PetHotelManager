using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Data;
using PetHotelManager.Models;
using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Pages.Products
{
    [Authorize(Roles = "Admin,Staff")]
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
            public int Id { get; set; }
            [Required] public string Name { get; set; }
            [Range(0, double.MaxValue)] public decimal Price { get; set; }
            [Display(Name = "Số lượng tồn kho")]
            [Range(0, int.MaxValue)] public int StockQuantity { get; set; }
            [Required] public string Unit { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            Input = new InputModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Unit = product.Unit
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var productToUpdate = await _context.Products.FindAsync(Input.Id);
            if (productToUpdate == null) return NotFound();

            productToUpdate.Name = Input.Name;
            productToUpdate.Price = Input.Price;
            productToUpdate.StockQuantity = Input.StockQuantity;
            productToUpdate.Unit = Input.Unit;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}