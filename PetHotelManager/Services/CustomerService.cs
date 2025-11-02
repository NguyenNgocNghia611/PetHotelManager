using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.Models;

namespace PetHotelManager.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ApplicationUser>> GetCustomersAsync(string? searchTerm)
        {
            var customerRoleId = await _context.Roles
                .Where(r => r.Name == "Customer")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (customerRoleId == null)
            {
                return new List<ApplicationUser>();
            }

            var customerUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == customerRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var query = _context.Users
                .Where(u => customerUserIds.Contains(u.Id))
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u =>
                    u.FullName.Contains(searchTerm) ||
                    u.PhoneNumber.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm));
            }

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }
    }
}