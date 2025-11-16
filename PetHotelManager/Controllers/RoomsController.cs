using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("update-status/{roomId}")]
        public async Task<IActionResult> UpdateRoomStatus(int roomId, [FromBody] string status)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng." });

            room.Status = status;
            await _context.SaveChangesAsync();

            var dto = new RoomStatusDto
            {
                RoomId = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType.TypeName,
                Status = room.Status
            };

            return Ok(new { message = $"Đã cập nhật trạng thái phòng thành '{status}'.", data = dto });
        }

        // tìm danh sách phòng trống thoe thowif gian

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
                return BadRequest(new { message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc." });

            // Lấy tất cả phòng
            var rooms = await _context.Rooms
                .Include(r => r.RoomType)
                .ToListAsync();

            // Lọc phòng có lịch hẹn trùng thời gian
            var bookedRoomIds = await _context.Appointments
                .Where(a => a.RoomId != null &&
                            a.Status != "Cancelled" &&
                            a.Status != "Rejected" &&
                            (
                                (a.CheckInDate <= endDate && a.CheckOutDate >= startDate) ||
                                (a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                            ))
                .Select(a => a.RoomId.Value)
                .Distinct()
                .ToListAsync();

            var availableRooms = rooms
                .Where(r => !bookedRoomIds.Contains(r.Id) && r.Status == "Available")
                .Select(r => new
                {
                    r.Id,
                    r.RoomNumber,
                    RoomType = r.RoomType.TypeName,
                    r.RoomType.PricePerDay,
                    r.Status
                })
                .ToList();

            return Ok(new
            {
                total = availableRooms.Count,
                data = availableRooms
            });
        }
    }
}
