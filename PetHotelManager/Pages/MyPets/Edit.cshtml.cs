using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;

namespace PetHotelManager.Pages.MyPets
{
    [Authorize(Roles = "Customer")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty] public EditPetForm Form { get; set; } = new();

        public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Forbid();

                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == Id && p.UserId == userId);
                if (pet == null)
                {
                    Error = "Không tìm thấy thú cưng hoặc bạn không có quyền.";
                    return Page();
                }

                Form = new EditPetForm
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Species = pet.Species,
                    Breed = pet.Breed,
                    HealthStatus = pet.HealthStatus
                };
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Forbid();

                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == Form.Id && p.UserId == userId);
                if (pet == null)
                {
                    Error = "Không tìm thấy thú cưng hoặc bạn không có quyền.";
                    return Page();
                }

                pet.Name = Form.Name;
                pet.Species = Form.Species;
                pet.Breed = Form.Breed;
                pet.HealthStatus = Form.HealthStatus;

                await _context.SaveChangesAsync();
                return RedirectToPage("/MyPets/Index");
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return Page();
            }
        }

        public class EditPetForm
        {
            public int Id { get; set; }

            [Required, StringLength(100)]
            public string Name { get; set; } = "";

            [Required, StringLength(50)]
            public string Species { get; set; } = "";

            [StringLength(100)]
            public string? Breed { get; set; }

            [StringLength(200)]
            public string? HealthStatus { get; set; }
        }
    }
}