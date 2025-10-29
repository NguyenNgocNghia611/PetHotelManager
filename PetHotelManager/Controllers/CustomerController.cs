using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PetHotelManager.DTOs.Auth;
using PetHotelManager.Models;

namespace PetHotelManager.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public CustomerController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    //POST: api/customer/register
    [HttpPost("register")]
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
}
