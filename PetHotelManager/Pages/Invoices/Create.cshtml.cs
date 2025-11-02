using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Invoices;
using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.Pages.Invoices
{
    using Microsoft.EntityFrameworkCore;

    [Authorize(Roles = "Admin,Staff")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public Dictionary<int, decimal> ServicePrices { get; set; } = new Dictionary<int, decimal>();
        public Dictionary<int, decimal> ProductPrices { get; set; } = new Dictionary<int, decimal>();

        public CreateModel(ApplicationDbContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList Customers { get; set; }
        public SelectList Services { get; set; }
        public SelectList Products { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng chọn khách hàng.")]
            [Display(Name = "Khách hàng")]
            public string UserId { get; set; }

        }

        public async Task OnGetAsync()
        {
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            var customers = new List<Models.ApplicationUser>();
            if (customerRole != null)
            {
                var customerIds = await _context.UserRoles
                                            .Where(ur => ur.RoleId == customerRole.Id)
                                            .Select(ur => ur.UserId)
                                            .ToListAsync();
                customers = await _context.Users
                                        .Where(u => customerIds.Contains(u.Id))
                                        .ToListAsync();
            }

            Customers = new SelectList(customers, "Id", "FullName");
            Services = new SelectList(_context.Services, "Id", "Name");
            Products = new SelectList(_context.Products, "Id", "Name");

            ServicePrices = await _context.Services.ToDictionaryAsync(s => s.Id, s => s.Price);
            ProductPrices = await _context.Products.ToDictionaryAsync(p => p.Id, p => p.Price);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return Page();
        }
    }
}