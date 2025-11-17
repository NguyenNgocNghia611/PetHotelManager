using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;

namespace PetHotelManager.Pages.MyPets
{
    [Authorize(Roles = "Customer")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PetDetailsViewModel Pet { get; set; } = null!;
        public List<MedicalRecordViewModel> MedicalRecords { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            var pet = await _context.Pets
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id.Value);

            if (pet == null)
            {
                return NotFound();
            }

            // Check if the pet belongs to the current user
            if (pet.UserId != userId)
            {
                return Forbid();
            }

            Pet = new PetDetailsViewModel
            {
                Id = pet.Id,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Age = pet.Age,
                Color = pet.Color,
                HealthStatus = pet.HealthStatus,
                ImageUrl = pet.ImageUrl
            };

            // Load medical records for this pet
            MedicalRecords = await _context.MedicalRecords
                .Where(m => m.PetId == id.Value)
                .Include(m => m.Veterinarian)
                .OrderByDescending(m => m.ExaminationDate)
                .Select(m => new MedicalRecordViewModel
                {
                    Id = m.Id,
                    ExaminationDate = m.ExaminationDate,
                    Symptoms = m.Symptoms,
                    Diagnosis = m.Diagnosis,
                    VeterinarianName = m.Veterinarian != null ? m.Veterinarian.FullName : "N/A"
                })
                .ToListAsync();

            return Page();
        }

        public class PetDetailsViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Species { get; set; } = "";
            public string? Breed { get; set; }
            public int Age { get; set; }
            public string? Color { get; set; }
            public string? HealthStatus { get; set; }
            public string? ImageUrl { get; set; }
        }

        public class MedicalRecordViewModel
        {
            public int Id { get; set; }
            public DateTime ExaminationDate { get; set; }
            public string Symptoms { get; set; } = "";
            public string Diagnosis { get; set; } = "";
            public string VeterinarianName { get; set; } = "";
        }
    }
}
