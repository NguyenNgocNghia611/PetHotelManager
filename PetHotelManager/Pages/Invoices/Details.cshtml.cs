using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.Pages.Invoices;
using PetHotelManager.Services;

namespace PetHotelManager.Pages.Invoices
{
    [Authorize] // Backend đã kiểm soát quyền bằng ownership + roles
    public class DetailsModel : PageModel
    {
        private readonly IInvoiceService _invoiceService;

        public DetailsModel(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public InvoiceDetailViewModel? Invoice { get; set; }
        public string?                 Error   { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Invoice = await _invoiceService.GetInvoiceDetailsAsync(Id);
                if (Invoice == null)
                {
                    Error = "Không tìm thấy hóa đơn.";
                }
            }
            catch (System.Exception ex)
            {
                Error = ex.Message;
            }
            return Page();
        }
    }
}