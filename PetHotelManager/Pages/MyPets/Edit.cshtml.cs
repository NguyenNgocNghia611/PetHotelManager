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
        private readonly IWebHostEnvironment _env;

        public EditModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty] public EditPetForm Form { get; set; } = new();

        public string? Error { get; set; }
        public string? ExistingImageUrl { get; set; }

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
                    Id           = pet.Id,
                    Name         = pet.Name,
                    Species      = pet.Species,
                    Breed        = pet.Breed,
                    HealthStatus = pet.HealthStatus,
                    Color        = pet.Color,
                    Age          = pet.Age
                };
                ExistingImageUrl = pet.ImageUrl;
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

                pet.Name         = Form.Name;
                pet.Species      = Form.Species;
                pet.Breed        = Form.Breed;
                pet.HealthStatus = Form.HealthStatus;
                pet.Color        = Form.Color ?? "";
                pet.Age          = Form.Age ?? pet.Age;

                if (Form.ImageFile != null && Form.ImageFile.Length > 0)
                {
                    if (!IsAllowed(Form.ImageFile))
                    {
                        ModelState.AddModelError(nameof(Form.ImageFile), "Định dạng ảnh không hợp lệ (jpg, jpeg, png, webp).");
                        return Page();
                    }

                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(pet.ImageUrl))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, pet.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    var folderPath = Path.Combine(_env.WebRootPath, "images", "pets");
                    Directory.CreateDirectory(folderPath);
                    var cleanFileName = Path.GetFileName(Form.ImageFile.FileName);
                    var fileName      = $"{Guid.NewGuid()}_{cleanFileName}";
                    var filePath      = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Form.ImageFile.CopyToAsync(stream);
                    }

                    pet.ImageUrl = $"/images/pets/{fileName}";
                }

                await _context.SaveChangesAsync();
                return RedirectToPage("/MyPets/Index");
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return Page();
            }
        }

        private bool IsAllowed(IFormFile file)
        {
            var ct = file.ContentType.ToLowerInvariant();
            return ct is "image/jpeg" or "image/jpg" or "image/png" or "image/webp";
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

            [StringLength(50)]
            public string? Color { get; set; }

            [Range(0, 100)]
            public int? Age { get; set; }

            public IFormFile? ImageFile { get; set; }
        }
    }
}