using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;
using PetHotelManager.Services;

namespace PetHotelManager.Pages.Invoices
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IInvoiceService _invoiceService;

        public IndexModel(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public PaginatedList<InvoiceViewModel> Invoices { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] string? userId, [FromQuery] int pageNumber = 1)
        {
            Invoices = await _invoiceService.GetInvoicesAsync(userId, pageNumber, 10);
            return Page();
        }
    }

    public class InvoiceViewModel
    {
        public int Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}