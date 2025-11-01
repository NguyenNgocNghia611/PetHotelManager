using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Customer;
using PetHotelManager.Models;

namespace PetHotelManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public CustomerManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ================================
        // F2.1. THÊM MỚI KHÁCH HÀNG
        // ================================
        [HttpPost("add")]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userExists = await _userManager.FindByNameAsync(dto.Username);
            if (userExists != null)
                return BadRequest(new { Message = "Tên đăng nhập đã tồn tại." });

            var newCustomer = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(newCustomer, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync("Customer"))
                await _roleManager.CreateAsync(new IdentityRole("Customer"));

            await _userManager.AddToRoleAsync(newCustomer, "Customer");

            return Ok(new { Status = "Success", Message = "Thêm mới khách hàng thành công!", Data = newCustomer });
        }

        // ================================
        // F2.2. CẬP NHẬT THÔNG TIN KHÁCH HÀNG
        // ================================
        [HttpPut("{id}/update")]
        public async Task<IActionResult> UpdateCustomer(string id, [FromBody] CusUpdateDto dto)
        {
            var customer = await _userManager.FindByIdAsync(id);
            if (customer == null)
                return NotFound(new { Message = "Không tìm thấy khách hàng." });

            customer.FullName = dto.FullName;
            customer.Email = dto.Email;
            customer.PhoneNumber = dto.PhoneNumber;
            customer.IsActive = dto.IsActive;

            var result = await _userManager.UpdateAsync(customer);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Status = "Success", Message = "Cập nhật thông tin thành công!" });
        }

    // ================================
    // F2.3. TÌM KIẾM & XEM DANH SÁCH KHÁCH HÀNG
    // ================================
    [HttpGet("list")]
        public async Task<IActionResult> GetCustomers([FromQuery] string? keyword)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u =>
                    u.FullName.Contains(keyword) ||
                    u.PhoneNumber.Contains(keyword));
            }

            var customers = await query
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, RoleName = r.Name })
                .Where(x => x.RoleName == "Customer")
                .Select(x => new
                {
                    x.u.Id,
                    x.u.FullName,
                    x.u.Email,
                    x.u.PhoneNumber,
                    x.u.CreatedAt,
                    x.u.IsActive
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(customers);
        }

        // ================================
        // F2.4. XEM LỊCH SỬ KHÁCH HÀNG
        // ================================
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetCustomerHistory(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = "Không tìm thấy khách hàng." });

            var pets = await _context.Pets
                .Where(p => p.UserId == id)
                .Select(p => new { p.Id, p.Name, p.Species, p.Breed })
                .ToListAsync();

            var invoices = await _context.Invoices
                .Where(i => i.UserId == id)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceDate,
                    i.TotalAmount,
                    i.Status
                })
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return Ok(new
            {
                Customer = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber
                },
                Pets = pets,
                Invoices = invoices
            });
        }
    }
}
