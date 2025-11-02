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
        public async Task<InvoiceDetailViewModel?> GetInvoiceDetailsAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.User)
                .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Product)
                .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Service)
                .Where(i => i.Id == invoiceId)
                .Select(i => new InvoiceDetailViewModel
                {
                    Id          = i.Id,
                    InvoiceDate = i.InvoiceDate,
                    TotalAmount = i.TotalAmount,
                    Status      = i.Status,
                    Customer = new CustomerViewModel
                    {
                        Id    = i.User.Id,
                        Name  = i.User.FullName,
                        Phone = i.User.PhoneNumber
                    },
                    Details = i.InvoiceDetails.Select(d => new ItemDetailViewModel
                    {
                        Description = d.Description,
                        Quantity    = d.Quantity,
                        UnitPrice   = d.UnitPrice,
                        SubTotal    = d.SubTotal,
                        ItemName    = d.Service != null ? d.Service.Name : (d.Product != null ? d.Product.Name : "")
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return invoice;
        }
    }
}