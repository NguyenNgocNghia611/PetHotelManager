using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PetHotelManager.Controllers
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using AutoMapper;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.IdentityModel.Tokens;
    using PetHotelManager.DTOs.Auth;
    using PetHotelManager.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>    _roleManager;
        private readonly IConfiguration               _configuration;
        private readonly IMapper                      _mapper;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Tài khoản của bạn đã bị vô hiệu hóa." });
                }

                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = CreateToken(authClaims);

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = userRoles;

                return Ok(new LoginResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    User = userDto
                });
            }

            return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
        }

        // This endpoint is used by Admin only to create Staff accounts.
        //It should be protected with role-based authorization and inside AdminController.
        [HttpPost("register-staff")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterDto registerDto)
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

            if (!await _roleManager.RoleExistsAsync("Staff"))
                 await _roleManager.CreateAsync(new IdentityRole("Staff"));

            await _userManager.AddToRoleAsync(user, "Staff");

            return Ok(new { Status = "Success", Message = "Tạo tài khoản nhân viên thành công!" });
        }


        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}