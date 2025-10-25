namespace PetHotelManager.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Profiles;
using PetHotelManager.Models;

[ApiController]
[Route("api/users/{userId}/profile")]
[Authorize(Roles = "Admin")]
public class ProfilesController : ControllerBase
{
    private readonly ApplicationDbContext         _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper                      _mapper;

    public ProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _context     = context;
        _userManager = userManager;
        _mapper      = mapper;
    }

    // GET: api/users/{userId}/profile
    [HttpGet]
    public async Task<IActionResult> GetProfile(string userId)
    {
        if (await _userManager.FindByIdAsync(userId) == null)
        {
            return NotFound(new { Message = "Không tìm thấy người dùng này." });
        }

        var profile = await _context.EmployeeProfiles.FindAsync(userId);

        if (profile == null)
        {
            return Ok(new EmployeeProfileDto { Id = userId });
        }

        return Ok(_mapper.Map<EmployeeProfileDto>(profile));
    }

    // PUT: api/users/{userId}/profile
    [HttpPut]
    public async Task<IActionResult> CreateOrUpdateProfile(string userId, [FromBody] UpdateEmployeeProfileDto profileDto)
    {
        if (await _userManager.FindByIdAsync(userId) == null)
        {
            return NotFound(new { Message = "Không tìm thấy người dùng này." });
        }

        var profile = await _context.EmployeeProfiles.FindAsync(userId);

        if (profile == null)
        {
            var newProfile = _mapper.Map<EmployeeProfile>(profileDto);
            newProfile.Id = userId; // Gán ID của user cho hồ sơ
            _context.EmployeeProfiles.Add(newProfile);
        }
        else
        {
            _mapper.Map(profileDto, profile);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}