using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;

namespace PetHotelManager.Pages.MyPets
{
    [Authorize(Roles = "Customer")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PetRow> Items { get; set; } = new();
        public string? Error { get; set; }
        public string? Success { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Forbid();

                Items = await _context.Pets
                    .Where(p => p.UserId == userId)
                    .OrderBy(p => p.Name)
                    .Select(p => new PetRow
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Species = p.Species,
                        Breed = p.Breed,
                        HealthStatus = p.HealthStatus
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Forbid();

                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
                if (pet == null)
                {
                    Error = "Không tìm thấy thú cưng hoặc bạn không có quyền.";
                    return await OnGetAsync();
                }

                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
                Success = "Đã xóa thú cưng.";
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return await OnGetAsync();
        }

        public class PetRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Species { get; set; } = "";
            public string? Breed { get; set; }
            public DateTime? BirthDate { get; set; }
            public string? HealthStatus { get; set; }
        }
    }
}