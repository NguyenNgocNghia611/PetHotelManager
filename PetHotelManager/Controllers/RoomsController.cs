using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Rooms;
using PetHotelManager.Models;

namespace PetHotelManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Rooms
                .Include(r => r.RoomType)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomTypeName = r.RoomType.TypeName,
                    PricePerDay = r.RoomType.PricePerDay,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(list);
        }

        // Get detail
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return NotFound();

            return Ok(new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomTypeName = room.RoomType.TypeName,
                PricePerDay = room.RoomType.PricePerDay,
                Status = room.Status
            });
        }

        // Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
        {
            var entity = new Room
            {
                RoomNumber = dto.RoomNumber,
                RoomTypeId = dto.RoomTypeId,
                Status = dto.Status
            };

            _context.Rooms.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Room created successfully", entity.Id });
        }

        // Update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Id mismatch");

            var entity = await _context.Rooms.FindAsync(id);
            if (entity == null)
                return NotFound();

            entity.RoomNumber = dto.RoomNumber;
            entity.RoomTypeId = dto.RoomTypeId;
            entity.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Room updated successfully" });
        }

        // Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Rooms.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.Rooms.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Room deleted successfully" });
        }
    }
}
