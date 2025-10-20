namespace PetHotelManager.Helpers;

using AutoMapper;
using PetHotelManager.DTOs.Auth;
using PetHotelManager.Models;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ApplicationUser, UserDto>();
    }
}