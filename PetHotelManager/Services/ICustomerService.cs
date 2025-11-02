namespace PetHotelManager.Services;

using PetHotelManager.Models;

public interface ICustomerService
{
    Task<List<ApplicationUser>> GetCustomersAsync(string? searchTerm);

}