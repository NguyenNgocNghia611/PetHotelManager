using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Common;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Appointments;
using PetHotelManager.Models;

namespace PetHotelManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tạo mới lịch hẹn
        //[Authorize(Roles = "Customer,Staff")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = new Appointment
            {
                UserId = dto.UserId,
                PetId = dto.PetId,
                ServiceId = dto.ServiceId,
                RoomId = dto.RoomId,
                AppointmentDate = dto.AppointmentDate,
                Notes = dto.Notes,
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt lịch thành công!", appointment.Id });
        }

        // Người đặt HUỶ lịch
        [Authorize(Roles = "Customer,Staff")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            if (appointment.Status == "Cancelled")
                return BadRequest(new { message = "Lịch hẹn đã bị hủy trước đó." });

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã hủy lịch hẹn thành công." });
        }

        // Người nhận TỪ CHỐI lịch
        //[Authorize(Roles = "Staff,Veterinarian")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            if (appointment.Status != "Pending")
                return BadRequest(new { message = "Chỉ có thể từ chối lịch hẹn đang chờ." });

            appointment.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã từ chối lịch hẹn." });
        }

        // Người nhận CHẤP NHẬN lịch
        [Authorize(Roles = "Staff,Veterinarian")]
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> AcceptAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            if (appointment.Status != "Pending")
                return BadRequest(new { message = "Chỉ có thể chấp nhận lịch hẹn đang chờ." });

            appointment.Status = "Accepted";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã chấp nhận lịch hẹn." });
        }

        // Danh sách tất cả lịch hẹn
        //[Authorize(Roles = "Admin,Staff,Veterinarian")]
        [HttpGet]
        public async Task<ActionResult> GetAllAppointments(
            string? search,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var pagination = new Pagination(pageNumber, pageSize);

            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.Room)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim().ToLower();
                query = query.Where(a =>
                    a.User.FullName.ToLower().Contains(keyword) ||
                    a.Pet.Name.ToLower().Contains(keyword) ||
                    a.Service.Name.ToLower().Contains(keyword));
            }

            pagination.TotalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(a => a.AppointmentDate)
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .Select(a => new AppointmentListDto
                {
                    Id = a.Id,
                    CustomerName = a.User.FullName,
                    PetName = a.Pet.Name,
                    ServiceName = a.Service.Name,
                    RoomName = a.Room != null
                        ? $"{a.Room.RoomNumber} - {a.Room.TypeName}"
                        : null,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(new
            {
                data,
                pagination = new
                {
                    pagination.PageNumber,
                    pagination.PageSize,
                    pagination.TotalRecords,
                    pagination.TotalPages
                }
            });
        }

        // Chi tiết 1 lịch hẹn
        //[Authorize(Roles = "Admin,Staff,Veterinarian,Customer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentDetail(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.Room)
                .Where(a => a.Id == id)
                .Select(a => new AppointmentDetailDto
                {
                    Id = a.Id,
                    CustomerName = a.User.FullName,
                    CustomerPhone = a.User.PhoneNumber,
                    PetName = a.Pet.Name,
                    ServiceName = a.Service.Name,
                    RoomName = a.Room != null
                        ? $"{a.Room.RoomNumber} - {a.Room.TypeName}"
                        : null,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Notes = a.Notes
                })
                .FirstOrDefaultAsync();

            if (appointment == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            return Ok(appointment);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterAppointmentsByStatus(
            [FromQuery] string status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var pagination = new Pagination(pageNumber, pageSize);

            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.Room)
                .Where(a => a.Status == status);

            var totalItems = await query.CountAsync();

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .Skip(pagination.Skip)
                .Take(pagination.Take)
                .Select(a => new AppointmentListDto
                {
                    Id = a.Id,
                    CustomerName = a.User.FullName,
                    PetName = a.Pet.Name,
                    ServiceName = a.Service.Name,
                    RoomName = a.Room.RoomNumber,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                pagination.PageNumber,
                pagination.PageSize,
                data = appointments
            });
        }
    }
}
