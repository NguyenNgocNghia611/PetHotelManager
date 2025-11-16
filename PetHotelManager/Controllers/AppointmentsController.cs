using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Common;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Appointments;
using PetHotelManager.DTOs.Rooms;
using PetHotelManager.Models;
using System.Security.Claims;

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
        [Authorize(Roles = "Customer,Staff")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //  Validate ngày 
            if (dto.CheckOutDate <= dto.AppointmentDate)
                return BadRequest(new { message = "Ngày trả phòng phải lớn hơn ngày nhận phòng." });

            //  Kiểm tra phòng có trống không 
            var hasConflict = await _context.Appointments
                .Where(a => a.RoomId == dto.RoomId && a.Status != "Cancelled")
                .AnyAsync(a =>
                    // Nếu đã check-in: trùng khoảng thời gian giữa checkin – checkout
                    (a.CheckInDate.HasValue && a.CheckOutDate.HasValue &&
                     a.CheckInDate.Value < dto.CheckOutDate && a.CheckOutDate.Value > dto.AppointmentDate)
                    ||
                    // Nếu chưa check-in: trùng ngày dự kiến
                    (!a.CheckInDate.HasValue && a.AppointmentDate < dto.CheckOutDate && a.AppointmentDate >= dto.AppointmentDate)
                );

            if (hasConflict)
                return BadRequest(new { message = "Phòng này đã được đặt trong khoảng thời gian bạn chọn." });

            //  Tạo lịch 
            var appointment = new Appointment
            {
                UserId = dto.UserId.Trim(),
                PetId = dto.PetId,
                ServiceId = dto.ServiceId,
                RoomId = dto.RoomId,
                AppointmentDate = dto.AppointmentDate,
                CheckInDate = null,
                CheckOutDate = dto.CheckOutDate,
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

            // Check ownership if user is Customer
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Customer") && appointment.UserId != currentUserId)
                return Forbid();

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã hủy lịch hẹn thành công." });
        }

        // Người nhận TỪ CHỐI lịch
        [Authorize(Roles = "Staff,Veterinarian")]
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
        [Authorize(Roles = "Admin,Staff,Veterinarian")]
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
                        ? $"{a.Room.RoomNumber} - {a.Room.RoomType.TypeName}"
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
        [Authorize(Roles = "Admin,Staff,Veterinarian,Customer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentDetail(int id)
        {
            var appointmentEntity = await _context.Appointments.FindAsync(id);
            if (appointmentEntity == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            // Check ownership if user is Customer
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Customer") && appointmentEntity.UserId != currentUserId)
                return Forbid();

            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.Room)
                    .ThenInclude(r => r.RoomType)
                .Where(a => a.Id == id)
                .Select(a => new AppointmentDetailDto
                {
                    Id = a.Id,
                    CustomerName = a.User.FullName,
                    CustomerPhone = a.User.PhoneNumber,
                    PetName = a.Pet.Name,
                    ServiceName = a.Service.Name,
                    RoomName = a.Room != null
                        ? $"{a.Room.RoomNumber} - {a.Room.RoomType.TypeName}"
                        : null,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Notes = a.Notes
                })
                .FirstOrDefaultAsync();

            return Ok(appointment);
        }

        //  CHECK-IN
        [Authorize(Roles = "Admin,Staff,Veterinarian")]
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

        [Authorize(Roles = "Admin,Staff,Veterinarian")]
        [HttpPost("checkin/{appointmentId}")]
        public async Task<IActionResult> CheckIn(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Room).ThenInclude(r => r.RoomType)
                .Include(a => a.Pet)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            if (appointment.Room == null)
                return BadRequest(new { message = "Lịch hẹn chưa được gán phòng." });

            if (appointment.Status == "CheckedIn")
                return BadRequest(new { message = "Lịch hẹn đã được check-in." });

            appointment.Status = "CheckedIn";
            appointment.CheckInDate = DateTime.Now;
            appointment.Room.Status = "Occupied";

            await _context.SaveChangesAsync();

            var dto = new RoomStatusDto
            {
                RoomId = appointment.Room.Id,
                RoomNumber = appointment.Room.RoomNumber,
                RoomType = appointment.Room.RoomType.TypeName,
                Status = appointment.Room.Status,
                CurrentAppointmentId = appointment.Id,
                PetName = appointment.Pet.Name,
                CustomerName = appointment.User.FullName,
                CheckInDate = appointment.CheckInDate
            };

            return Ok(new { message = "Check-in thành công.", data = dto });
        }

        //  CHECK-OUT
        [Authorize(Roles = "Admin,Staff,Veterinarian")]
        [HttpPost("checkout/{appointmentId}")]
        public async Task<IActionResult> CheckOut(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Room).ThenInclude(r => r.RoomType)
                .Include(a => a.Pet)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            if (appointment.Room == null)
                return BadRequest(new { message = "Lịch hẹn chưa được gán phòng." });

            if (appointment.Status != "CheckedIn")
                return BadRequest(new { message = "Chỉ có thể check-out lịch hẹn đã check-in." });

            appointment.Status = "CheckedOut";
            appointment.CheckOutDate = DateTime.Now;
            appointment.Room.Status = "Available";

            await _context.SaveChangesAsync();

            var dto = new RoomStatusDto
            {
                RoomId = appointment.Room.Id,
                RoomNumber = appointment.Room.RoomNumber,
                RoomType = appointment.Room.RoomType.TypeName,
                Status = appointment.Room.Status,
                CurrentAppointmentId = appointment.Id,
                PetName = appointment.Pet.Name,
                CustomerName = appointment.User.FullName,
                CheckInDate = appointment.CheckInDate,
                CheckOutDate = appointment.CheckOutDate
            };

            return Ok(new { message = "Check-out thành công.", data = dto });
        }

    }
}
