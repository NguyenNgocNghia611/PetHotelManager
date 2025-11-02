using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.Pages.Invoices;

namespace PetHotelManager.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<InvoiceViewModel>> GetInvoicesAsync(string? userId, int pageNumber, int pageSize)
        {
            var query = _context.Invoices
                .Include(i => i.User)
                .OrderByDescending(i => i.InvoiceDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(i => i.UserId == userId);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InvoiceViewModel
                {
                    Id           = i.Id,
                    InvoiceDate  = i.InvoiceDate,
                    TotalAmount  = i.TotalAmount,
                    Status       = i.Status,
                    CustomerName = i.User.FullName,
                    CustomerId   = i.UserId
                })
                .ToListAsync();

            return new PaginatedList<InvoiceViewModel>(items, totalRecords, pageNumber, pageSize);
        }
    }
}