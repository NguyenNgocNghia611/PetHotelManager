namespace PetHotelManager.Helpers;

using AutoMapper;
using PetHotelManager.DTOs.Admin;
using PetHotelManager.DTOs.Auth;
using PetHotelManager.DTOs.MedicalRecord;
using PetHotelManager.DTOs.Prescription;
using PetHotelManager.Models;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<ApplicationUser, UserManagementDto>();

        // MedicalRecord -> MedicalRecordDto
        CreateMap<MedicalRecord, MedicalRecordDto>()
            .ForMember(dest => dest.VeterinarianName, opt => opt.MapFrom(src => src.Veterinarian != null ? src.Veterinarian.FullName : null))
            .ForMember(dest => dest.Prescriptions, opt => opt.MapFrom(src => src.PrescriptionDetails));

        // PrescriptionDetail -> PrescriptionDetailDto
        CreateMap<PrescriptionDetail, PrescriptionDetailDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
    }
}