using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Pets;
using PetHotelManager.Models;

namespace PetHotelManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================
        // F3.1: Thêm mới thú cưng
        // ================================
        [Authorize(Roles = "Staff,Veterinarian")]
        [HttpPost]
        public async Task<IActionResult> AddPet([FromForm] CreatePetDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? imageUrl = null;

            // Xử lý upload ảnh nếu có
            if (dto.ImageFile != null)
            {
                var fileName = $"{Guid.NewGuid()}_{dto.ImageFile.FileName}";
                var folderPath = Path.Combine("wwwroot", "images", "pets");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/images/pets/{fileName}";
            }

            var pet = new Pet
            {
                UserId = dto.UserId,
                Name = dto.Name,
                Species = dto.Species,
                Breed = dto.Breed,
                Age = dto.Age,
                Color = dto.Color,
                HealthStatus = dto.HealthStatus,
                ImageUrl = imageUrl
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm mới thú cưng thành công!", pet.Id, pet.ImageUrl });
        }


        // ================================
        // F3.2: Cập nhật thông tin
        // ================================
        [Authorize(Roles = "Staff,Veterinarian")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(int id, [FromForm] UpdatePetDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Id không khớp");

            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
                return NotFound();

            pet.Name = dto.Name;
            pet.Species = dto.Species;
            pet.Breed = dto.Breed;
            pet.Age = dto.Age;
            pet.Color = dto.Color;
            pet.HealthStatus = dto.HealthStatus;

            if (dto.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(pet.ImageUrl))
                {
                    var oldPath = Path.Combine("wwwroot", pet.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = $"{Guid.NewGuid()}_{dto.ImageFile.FileName}";
                var folderPath = Path.Combine("wwwroot", "images", "pets");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                pet.ImageUrl = $"/images/pets/{fileName}";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thông tin thú cưng thành công!", pet.ImageUrl });
        }


        // ================================
        // F3.3: Xem danh sách thú cưng
        // ================================
        [Authorize(Roles = "Admin,Staff,Veterinarian")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PetListDto>>> GetAllPets()
        {
            var pets = await _context.Pets
                .Include(p => p.User)
                .Select(p => new PetListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Species = p.Species,
                    ImageUrl = p.ImageUrl,
                    OwnerName = p.User.FullName
                })
                .ToListAsync();

            return Ok(pets);
        }

        [Authorize(Roles = "Admin,Staff,Veterinarian")]
        [HttpGet("{id}")]
        public async Task<ActionResult<PetDetailDto>> GetPetDetail(int id)
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                .Where(p => p.Id == id)
                .Select(p => new PetDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Species = p.Species,
                    Breed = p.Breed,
                    Age = p.Age,
                    Color = p.Color,
                    HealthStatus = p.HealthStatus,
                    ImageUrl = p.ImageUrl,
                    OwnerName = p.User.FullName,
                    OwnerPhone = p.User.PhoneNumber
                })
                .FirstOrDefaultAsync();

            if (pet == null)
                return NotFound(new { message = "Không tìm thấy thú cưng." });

            return Ok(pet);
        }
    }
}