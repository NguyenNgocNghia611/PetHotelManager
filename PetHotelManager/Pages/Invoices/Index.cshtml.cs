using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Pages.Invoices;
using PetHotelManager.Services;

namespace PetHotelManager.Pages.Invoices
{
    [Authorize] // quyền chi tiết đã kiểm soát ở API, trang yêu cầu đăng nhập
    public class IndexModel : PageModel
    {
        private readonly IInvoiceService _invoiceService;

        public IndexModel(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public PaginatedList<InvoiceViewModel>? Invoices { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int     TotalRecords   => Invoices == null ? 0 : ((Invoices.TotalPages > 0) ? Invoices.TotalPages * PageSize : Invoices.Count);
        public bool    IsAdminOrStaff => User.IsInRole("Admin") || User.IsInRole("Staff");
        public string? Error          { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // UserId: chỉ meaningful cho Admin/Staff; Customer sẽ bị API backend override (ownership)
                var effectiveUserId = IsAdminOrStaff ? UserId : null;
                Invoices = await _invoiceService.GetInvoicesAsync(effectiveUserId, PageNumber, PageSize);
            }
            catch (System.Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }
    }
}