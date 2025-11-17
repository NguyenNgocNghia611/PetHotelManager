using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Data;
using PetHotelManager.Models;

namespace PetHotelManager.Pages.MyPets
{
    [Authorize(Roles = "Customer")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] public CreatePetForm Form { get; set; } = new();

        public string? Error { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Forbid();

                var pet = new Pet
                {
                    Name = Form.Name,
                    Species = Form.Species,
                    Breed = Form.Breed,
                    HealthStatus = Form.HealthStatus,
                    UserId = userId
                };

                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();
                return RedirectToPage("/MyPets/Index");
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return Page();
            }
        }

        public class CreatePetForm
        {
            [Required, StringLength(100)]
            public string Name { get; set; } = "";

            [Required, StringLength(50)]
            public string Species { get; set; } = "";

            [StringLength(100)]
            public string? Breed { get; set; }

            public DateTime? BirthDate { get; set; }

            [StringLength(200)]
            public string? HealthStatus { get; set; }
        }
    }
}