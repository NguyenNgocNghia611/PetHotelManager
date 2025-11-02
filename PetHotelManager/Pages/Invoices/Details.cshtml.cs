using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace PetHotelManager.Pages.Invoices
{
    using PetHotelManager.Services;

    [Authorize(Roles = "Admin,Staff")]
    public class DetailsModel : PageModel
    {
        private readonly IInvoiceService _invoiceService;

        public DetailsModel(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public InvoiceDetailViewModel? Invoice { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Invoice = await _invoiceService.GetInvoiceDetailsAsync(id);

            if (Invoice == null)
            {
                return NotFound();
            }

            return Page();
        }
    }

    public class InvoiceDetailViewModel
    {
        public int Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public CustomerViewModel Customer { get; set; }
        public List<ItemDetailViewModel> Details { get; set; }
    }

    public class CustomerViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    public class ItemDetailViewModel
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public string ItemName { get; set; }
    }
}