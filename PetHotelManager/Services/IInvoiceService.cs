using PetHotelManager.Pages.Invoices;

namespace PetHotelManager.Services
{
    public interface IInvoiceService
    {
        Task<PaginatedList<InvoiceViewModel>> GetInvoicesAsync(string?   userId, int pageNumber, int pageSize);
        Task<InvoiceDetailViewModel?>         GetInvoiceDetailsAsync(int invoiceId);

    }
}