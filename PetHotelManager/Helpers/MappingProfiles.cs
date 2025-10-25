namespace PetHotelManager.Helpers;

using AutoMapper;
using PetHotelManager.DTOs.Admin;
using PetHotelManager.DTOs.Auth;
using PetHotelManager.DTOs.Profiles;
using PetHotelManager.Models;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<ApplicationUser, UserManagementDto>();
        CreateMap<EmployeeProfile, EmployeeProfileDto>();
        CreateMap<UpdateEmployeeProfileDto, EmployeeProfile>();
    }
}