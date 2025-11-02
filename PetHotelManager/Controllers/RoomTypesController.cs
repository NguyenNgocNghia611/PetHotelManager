using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.RoomTypes;
using PetHotelManager.Models;

namespace PetHotelManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.RoomTypes
                .Select(rt => new RoomTypeDto
                {
                    Id = rt.Id,
                    TypeName = rt.TypeName,
                    PricePerDay = rt.PricePerDay,
                    Description = rt.Description
                })
                .ToListAsync();

            return Ok(list);
        }

        // Get detail
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var type = await _context.RoomTypes.FindAsync(id);
            if (type == null)
                return NotFound();

            return Ok(new RoomTypeDto
            {
                Id = type.Id,
                TypeName = type.TypeName,
                PricePerDay = type.PricePerDay,
                Description = type.Description
            });
        }

        // Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomTypeDto dto)
        {
            var entity = new RoomType
            {
                TypeName = dto.TypeName,
                PricePerDay = dto.PricePerDay,
                Description = dto.Description
            };

            _context.RoomTypes.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Room type created successfully", entity.Id });
        }

        // Update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomTypeDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Id mismatch");

            var entity = await _context.RoomTypes.FindAsync(id);
            if (entity == null)
                return NotFound();

            entity.TypeName = dto.TypeName;
            entity.PricePerDay = dto.PricePerDay;
            entity.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Room type updated successfully" });
        }

        // Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.RoomTypes.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.RoomTypes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Room type deleted successfully" });
        }
    }
}
