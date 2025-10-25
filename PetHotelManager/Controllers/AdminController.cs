using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace PetHotelManager.Controllers
{
    using System.Security.Claims;
    using AutoMapper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using PetHotelManager.DTOs.Admin;
    using PetHotelManager.Models;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AdminController(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserManagementDto>();

            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserManagementDto>(user);
                userDto.Roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(userDto);
            }
            return Ok(userDtos);
        }

        // GET: api/Admin/users/{id}
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "Không tìm thấy người dùng." });
            }

            var userDto = _mapper.Map<UserManagementDto>(user);
            userDto.Roles = await _userManager.GetRolesAsync(user);

            return Ok(userDto);
        }

        // POST: api/Admin/users
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserDto createUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (createUserDto.Role != "Staff" && createUserDto.Role != "Doctor")
            {
                return BadRequest(new { Message = "Vai trò không hợp lệ. Chỉ chấp nhận 'Staff' hoặc 'Doctor'." });
            }

            var userExists = await _userManager.FindByNameAsync(createUserDto.Username);
            if (userExists != null) return Conflict(new { Message = "Tên đăng nhập đã tồn tại." });

            var newUser = new ApplicationUser
            {
                UserName = createUserDto.Username,
                Email = createUserDto.Email,
                FullName = createUserDto.FullName,
                PhoneNumber = createUserDto.PhoneNumber, // Xử lý PhoneNumber
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(newUser, createUserDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(newUser, createUserDto.Role);

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }

        // PUT: api/Admin/users/{id}
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] AdminUpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { Message = "Không tìm thấy người dùng." });

            user.Email = updateUserDto.Email;
            user.FullName = updateUserDto.FullName;
            user.PhoneNumber = updateUserDto.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return BadRequest(updateResult.Errors);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, updateUserDto.Role);

            return NoContent();
        }

        // PUT: api/Admin/users/{id}/toggle-status
        [HttpPut("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { Message = "Không tìm thấy người dùng." });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id == currentUserId) return BadRequest(new { Message = "Bạn không thể vô hiệu hóa tài khoản của chính mình." });

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        // DELETE: api/admin/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { Message = "Không tìm thấy người dùng." });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id == currentUserId) return BadRequest(new { Message = "Bạn không thể xóa tài khoản của chính mình." });

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }
    }
}