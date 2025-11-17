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
        private readonly IWebHostEnvironment _env;

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
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

                string? imageUrl = null;
                if (Form.ImageFile != null && Form.ImageFile.Length > 0)
                {
                    if (!IsAllowed(Form.ImageFile))
                    {
                        ModelState.AddModelError(nameof(Form.ImageFile), "Định dạng ảnh không hợp lệ (chỉ jpg, jpeg, png, webp).");
                        return Page();
                    }

                    var folderPath = Path.Combine(_env.WebRootPath, "images", "pets");
                    Directory.CreateDirectory(folderPath);

                    var cleanFileName = Path.GetFileName(Form.ImageFile.FileName);
                    var fileName = $"{Guid.NewGuid()}_{cleanFileName}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Form.ImageFile.CopyToAsync(stream);
                    }

                    imageUrl = $"/images/pets/{fileName}";
                }

                var pet = new Pet
                {
                    Name         = Form.Name,
                    Species      = Form.Species,
                    Breed        = Form.Breed,
                    HealthStatus = Form.HealthStatus,
                    Color        = Form.Color ?? "",
                    Age          = Form.Age ?? 0,
                    UserId       = userId,
                    ImageUrl     = imageUrl
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

        private bool IsAllowed(IFormFile file)
        {
            var contentType = file.ContentType.ToLowerInvariant();
            return contentType is "image/jpeg" or "image/jpg" or "image/png" or "image/webp";
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

            [StringLength(50)]
            public string? Color { get; set; }

            [Range(0, 100)]
            public int? Age { get; set; }

            public IFormFile? ImageFile { get; set; }
        }
    }
}