namespace PetHotelManager.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Invoices;
using PetHotelManager.Models;

public class InvoicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InvoicesController(ApplicationDbContext context)
    {
        _context = context;
    }
    // POST: api/invoices
    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] InvoiceCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var customer = await _context.Users.FindAsync(createDto.UserId);
        if (customer == null)
        {
            return NotFound(new { Message = "Không tìm thấy khách hàng." });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var invoice = new Invoice
            {
                UserId      = createDto.UserId,
                InvoiceDate = DateTime.UtcNow,
                Status      = "Unpaid",
                TotalAmount = 0
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            foreach (var detailDto in createDto.Details)
            {
                decimal       unitPrice   = 0;
                string        description = "";
                InvoiceDetail invoiceDetail;

                if (detailDto.ServiceId.HasValue)
                {
                    var service = await _context.Services.FindAsync(detailDto.ServiceId.Value);
                    if (service == null) throw new Exception($"Dịch vụ với ID {detailDto.ServiceId} không tồn tại.");

                    unitPrice   = service.Price;
                    description = service.Name;

                    invoiceDetail = new InvoiceDetail
                    {
                        InvoiceId   = invoice.Id,
                        ServiceId   = detailDto.ServiceId,
                        Quantity    = detailDto.Quantity,
                        UnitPrice   = unitPrice,
                        Description = description,
                        SubTotal    = detailDto.Quantity * unitPrice
                    };
                }
                else if (detailDto.ProductId.HasValue)
                {
                    var product = await _context.Products.FindAsync(detailDto.ProductId.Value);
                    if (product == null) throw new Exception($"Sản phẩm với ID {detailDto.ProductId} không tồn-tại.");
                    if (product.StockQuantity < detailDto.Quantity) throw new Exception($"Không đủ tồn kho cho sản phẩm '{product.Name}'. Chỉ còn {product.StockQuantity}.");

                    unitPrice   = product.Price;
                    description = product.Name;

                    product.StockQuantity -= detailDto.Quantity;

                    invoiceDetail = new InvoiceDetail
                    {
                        InvoiceId   = invoice.Id,
                        ProductId   = detailDto.ProductId,
                        Quantity    = detailDto.Quantity,
                        UnitPrice   = unitPrice,
                        Description = description,
                        SubTotal    = detailDto.Quantity * unitPrice
                    };
                }
                else
                {
                    continue;
                }

                _context.InvoiceDetails.Add(invoiceDetail);
                invoice.TotalAmount += invoiceDetail.SubTotal;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Message = "Tạo hóa đơn thành công!", InvoiceId = invoice.Id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { Message = "Đã xảy ra lỗi trong quá trình tạo hóa đơn.", Error = ex.Message });
        }
    }
}