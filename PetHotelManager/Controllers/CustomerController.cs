using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Appointment;
using PetHotelManager.DTOs.Auth;
using PetHotelManager.DTOs.Customer;
using PetHotelManager.DTOs.Pets;
using PetHotelManager.Models;

namespace PetHotelManager.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public CustomerController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    #region Customer's stuff

    //POST: api/customer/register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterDto registerDto)
    {
        var userExists = await _userManager.FindByNameAsync(registerDto.Username);
        if (userExists != null)
            return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "Tên đăng nhập đã tồn tại!" });

        ApplicationUser user = new()
        {
            Email = registerDto.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = registerDto.Username,
            FullName = registerDto.FullName
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "Tạo người dùng thất bại! Vui lòng kiểm tra lại thông tin.", Details = result.Errors });

        if (!await _roleManager.RoleExistsAsync("Customer"))
            await _roleManager.CreateAsync(new IdentityRole("Customer"));

        await _userManager.AddToRoleAsync(user, "Customer");

        return Ok(new { Status = "Success", Message = "Đăng ký khách hàng thành công!" });
    }

    //PUT: api/customer/{id}/update
    [HttpPut("{id}/update")]
    public async Task<IActionResult> UpdateCustomer(string id, [FromBody] CustomerUpdateDto updateUserDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound(new { Message = "Không tìm thấy khách hàng." });

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains("Customer"))
            return BadRequest(new { Message = "Người dùng không phải khách hàng." });

        user.Email = updateUserDto.Email;
        user.FullName = updateUserDto.FullName;
        user.PhoneNumber = updateUserDto.PhoneNumber;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) return BadRequest(updateResult.Errors);

        return Ok(new { Status = "Success", Message = "Cập nhật thông tin khách hàng thành công!" });
    }

    //PUT: api/customer/{id}/change-password
    [HttpPut("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto changeDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return Unauthorized(new { Message = "Người dùng không được xác thực." });

        if (changeDto.NewPassword != changeDto.ConfirmPassword)
            return BadRequest(new { Message = "Mật khẩu mới và xác nhận không khớp." });

        var changeResult = await _userManager.ChangePasswordAsync(user, changeDto.CurrentPassword, changeDto.NewPassword);
        if (!changeResult.Succeeded)
            return BadRequest(new { Message = "Thay đổi mật khẩu thất bại.", changeResult.Errors });

        return Ok(new { Status = "Success", Message = "Thay đổi mật khẩu thành công!" });
    }

    #endregion

    #region Pet stuff

    // GET: api/customer/{id}/pets
    [HttpGet("{id}/pets")]
    public async Task<IActionResult> GetAllPets(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return Unauthorized(new { Message = "Người dùng không được xác thực." });

        var pets = await _context.Pets
            .Where(p => p.UserId == user.Id)
            .Include(p => p.MedicalRecords)
            .Include(p => p.Appointments)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Species,
                p.Breed,
                //MedicalRecords = p.MedicalRecords.Select(m => new MedicalRecordDto
                //{
                //    Id = m.Id,
                //    ExaminationDate = m.ExaminationDate,
                //    Diagnosis = m.Diagnosis,
                //    Symptoms = m.Symptoms,
                //    VeterinarianId = m.VeterinarianId
                //}).ToList(),
                //Appointments = p.Appointments.Select(a => new AppointmentDto
                //{
                //    Id = a.Id,
                //    AppointmentDate = a.AppointmentDate,
                //    Status = a.Status,
                //    Notes = a.Notes,
                //    CheckInDate = a.CheckInDate,
                //    CheckOutDate = a.CheckOutDate,
                //    ServiceId = a.ServiceId,
                //    RoomId = a.RoomId
                //}).ToList()
            })
            .ToListAsync();

        return Ok(pets);
    }

    // POST: api/customer/{id}/pets/add
    [HttpPost("{id}/pets/add")]
    public async Task<IActionResult> AddPet(string id, [FromBody] PetCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return Unauthorized(new { Message = "Người dùng không được xác thực." });

        var pet = new Pet
        {
            Name = dto.Name,
            Species = dto.Species ?? string.Empty,
            Breed = dto.Breed ?? string.Empty,
            UserId = user.Id
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        var petDto = new
        {
            pet.Id,
            pet.Name,
            pet.Species,
            pet.Breed,
            //MedicalRecords = new List<MedicalRecordDto>(),
            //Appointments = new List<AppointmentDto>()
        };

        return Ok(new { Status = "Success", Message = "Thêm thú cưng thành công!", Data = petDto });
    }

    #endregion

    #region Appointments stuff

    // POST: api/customer/{id}/appointments
    // Create a new appointment for the customer's pet.
    [HttpPost("{id}/appointments")]
    public async Task<IActionResult> CreateAppointment(string id, [FromBody] AppointmentCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return Unauthorized(new { Message = "Người dùng không được xác thực." });

        // Verify pet exists and belongs to user
        var pet = await _context.Pets.FindAsync(dto.PetId);
        if (pet == null || pet.UserId != user.Id)
            return BadRequest(new { Message = "Thú cưng không tồn tại hoặc không thuộc về người dùng." });

        // Optional: validate service/room existence
        if (dto.ServiceId.HasValue)
        {
            var serviceExists = await _context.Services.AnyAsync(s => s.Id == dto.ServiceId.Value);
            if (!serviceExists) return BadRequest(new { Message = "Dịch vụ không tồn tại." });
        }

        if (dto.RoomId.HasValue)
        {
            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == dto.RoomId.Value);
            if (!roomExists) return BadRequest(new { Message = "Phòng không tồn tại." });
        }

        if (dto.AppointmentDate <= DateTime.UtcNow)
            return BadRequest(new { Message = "Thời gian cuộc hẹn phải ở tương lai." });

        var appointment = new Appointment
        {
            UserId = user.Id,
            PetId = pet.Id,
            ServiceId = dto.ServiceId,
            RoomId = dto.RoomId,
            AppointmentDate = dto.AppointmentDate,
            Notes = dto.Notes ?? string.Empty,
            Status = "Pending"
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var result = new
        {
            appointment.Id,
            appointment.PetId,
            appointment.ServiceId,
            appointment.RoomId,
            appointment.AppointmentDate,
            appointment.Status,
            appointment.Notes
        };

        return Ok(new { Status = "Success", Message = "Lập lịch cuộc hẹn thành công.", Data = result });
    }

    // GET: api/customer/{id}/appointments/{appointmentId}/status
    [HttpGet("{id}/appointments/{appointmentId}/status")]
    public async Task<IActionResult> GetAppointmentStatus(string id, int appointmentId)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return Unauthorized(new { Message = "Người dùng không được xác thực." });

        var appointment = await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null) return NotFound(new { Message = "Cuộc hẹn không tồn tại." });
        if (appointment.UserId != user.Id) return Forbid();

        return Ok(new
        {
            appointment.Id,
            appointment.Status,
            appointment.AppointmentDate,
            appointment.CheckInDate,
            appointment.CheckOutDate,
            appointment.Notes
        });
    }

    // PUT: api/customer/{id}/appointments/{appointmentId}/cancel
    [HttpPut("{id}/appointments/{appointmentId}/cancel")]
    public async Task<IActionResult> CancelAppointment(string id, int appointmentId)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return Unauthorized(new { Message = "Người dùng không được xác thực." });

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        if (appointment == null) return NotFound(new { Message = "Cuộc hẹn không tồn tại." });
        if (appointment.UserId != user.Id) return Forbid();

        if (appointment.Status == "Cancelled")
            return BadRequest(new { Message = "Cuộc hẹn đã bị hủy trước đó." });

        if (appointment.Status == "Completed")
            return BadRequest(new { Message = "Không thể hủy cuộc hẹn đã hoàn tất." });

        appointment.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return Ok(new { Status = "Success", Message = "Hủy cuộc hẹn thành công.", Data = new { appointment.Id, appointment.Status } });
    }

    #endregion

    #region Medical history & Invoices

    // GET: api/customer/{id}/medical-records
    [HttpGet("{id}/medical-records")]
    public async Task<IActionResult> GetMedicalRecords(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return Unauthorized(new { Message = "Người dùng không được xác thực." });

        var records = await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Include(m => m.PrescriptionDetails)
                .ThenInclude(pd => pd.Product)
            .Where(m => m.Pet.UserId == user.Id)
            .OrderByDescending(m => m.ExaminationDate)
            .Select(m => new
            {
                m.Id,
                Pet = new { m.Pet.Id, m.Pet.Name },
                m.ExaminationDate,
                m.Symptoms,
                m.Diagnosis,
                Veterinarian = m.Veterinarian == null ? null : new { m.Veterinarian.Id, m.Veterinarian.FullName },
                Prescriptions = m.PrescriptionDetails.Select(pd => new
                {
                    pd.Id,
                    pd.ProductId,
                    ProductName = pd.Product != null ? pd.Product.Name : null,
                    pd.Dosage,
                    pd.Quantity
                }).ToList()
            })
            .ToListAsync();

        return Ok(records);
    }

    // GET: api/customer/{id}/invoices
    [HttpGet("{id}/invoices")]
    public async Task<IActionResult> GetInvoices(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return Unauthorized(new { Message = "Người dùng không được xác thực." });

        var invoices = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.UserId == user.Id)
            .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Product)
            .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Service)
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new
            {
                i.Id,
                i.InvoiceDate,
                i.TotalAmount,
                i.Status,
                Details = i.InvoiceDetails.Select(d => new
                {
                    d.Id,
                    d.Description,
                    d.Quantity,
                    d.UnitPrice,
                    d.SubTotal,
                    Product = d.Product == null ? null : new { d.Product.Id, d.Product.Name },
                    Service = d.Service == null ? null : new { d.Service.Id, d.Service.Name }
                }).ToList()
            })
            .ToListAsync();

        return Ok(invoices);
    }

    // GET: api/customer/{id}/invoices/{invoiceId}
    [HttpGet("{id}/invoices/{invoiceId}")]
    public async Task<IActionResult> GetInvoiceById(string id, int invoiceId)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return Unauthorized(new { Message = "Người dùng không được xác thực." });

        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Product)
            .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Service)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null) return NotFound(new { Message = "Hóa đơn không tồn tại." });
        if (invoice.UserId != user.Id) return Forbid();

        var result = new
        {
            invoice.Id,
            invoice.InvoiceDate,
            invoice.TotalAmount,
            invoice.Status,
            Details = invoice.InvoiceDetails.Select(d => new
            {
                d.Id,
                d.Description,
                d.Quantity,
                d.UnitPrice,
                d.SubTotal,
                Product = d.Product == null ? null : new { d.Product.Id, d.Product.Name },
                Service = d.Service == null ? null : new { d.Service.Id, d.Service.Name }
            }).ToList()
        };

        return Ok(result);
    }

    #endregion
}