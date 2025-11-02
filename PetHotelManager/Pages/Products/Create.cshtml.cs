using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Data;
using PetHotelManager.Models;
using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Pages.Products
{
    [Authorize(Roles = "Admin,Staff")]
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
            [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
            public string Name { get; set; }

            [Range(0, double.MaxValue)]
            public decimal Price { get; set; }

            [Display(Name = "Số lượng tồn kho")]
            [Range(0, int.MaxValue)]
            public int StockQuantity { get; set; }

            [Required(ErrorMessage = "Đơn vị tính là bắt buộc.")]
            public string Unit { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var product = new Product
            {
                Name          = Input.Name,
                Price         = Input.Price,
                StockQuantity = Input.StockQuantity,
                Unit          = Input.Unit
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}