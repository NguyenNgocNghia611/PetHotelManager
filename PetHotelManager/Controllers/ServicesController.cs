using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Service;
using PetHotelManager.Models;

namespace PetHotelManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }
        // ================================
        // F4.1 - Quản lý danh mục dịch vụ
        // QTV có thể thêm, sửa, xóa các loại dịch vụ (Spa, Grooming, Khách sạn theo ngày, Khám bệnh, Tiêm phòng...)
        // và thiết lập giá. Ai cũng có thể xem danh sách dịch vụ.
        // ================================
        /// <remarks>Ai cũng có thể xem danh sách dịch vụ.</remarks>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServices()
        {
            var services = await _context.Services
                .Select(s => new ServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    Price = s.Price,
                    Unit = s.Unit
                })
                .ToListAsync();

            return Ok(services);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceDto>> GetService(int id)
        {
            var service = await _context.Services
                .Where(s => s.Id == id)
                .Select(s => new ServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    Price = s.Price,
                    Unit = s.Unit
                })
                .FirstOrDefaultAsync();

            if (service == null)
                return NotFound();

            return Ok(service);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ServiceDto>> CreateService(CreateServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = new Service
            {
                Name = dto.Name,
                Category = dto.Category,
                Price = dto.Price,
                Unit = dto.Unit
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            var result = new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Category = service.Category,
                Price = service.Price,
                Unit = service.Unit
            };

            return CreatedAtAction(nameof(GetService), new { id = service.Id }, result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateService(int id, UpdateServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest("Id không khớp");

            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            service.Name = dto.Name;
            service.Category = dto.Category;
            service.Price = dto.Price;
            service.Unit = dto.Unit;

            _context.Entry(service).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công", updatedService = service });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa dịch vụ thành công" });
        }
    }
}
