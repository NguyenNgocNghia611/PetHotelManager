namespace PetHotelManager.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Invoices;
using PetHotelManager.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InvoicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/invoices
    [Authorize(Roles = "Admin,Staff")]
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

        // ⭐ Lấy userId để ghi log
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var invoice = new Invoice
            {
                UserId = createDto.UserId,
                InvoiceDate = DateTime.UtcNow,
                Status = "Unpaid",
                TotalAmount = 0
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            foreach (var detailDto in createDto.Details)
            {
                decimal unitPrice = 0;
                string description = "";
                InvoiceDetail invoiceDetail;

                if (detailDto.ServiceId.HasValue)
                {
                    var service = await _context.Services.FindAsync(detailDto.ServiceId.Value);
                    if (service == null)
                        throw new Exception($"Dịch vụ với ID {detailDto.ServiceId} không tồn tại.");

                    unitPrice = service.Price;
                    description = service.Name;

                    invoiceDetail = new InvoiceDetail
                    {
                        InvoiceId = invoice.Id,
                        ServiceId = detailDto.ServiceId,
                        Quantity = detailDto.Quantity,
                        UnitPrice = unitPrice,
                        Description = description,
                        SubTotal = detailDto.Quantity * unitPrice
                    };
                }
                else if (detailDto.ProductId.HasValue)
                {
                    var product = await _context.Products.FindAsync(detailDto.ProductId.Value);
                    if (product == null)
                        throw new Exception($"Sản phẩm với ID {detailDto.ProductId} không tồn tại.");

                    if (product.StockQuantity < detailDto.Quantity)
                        throw new Exception($"Không đủ tồn kho cho sản phẩm '{product.Name}'. Chỉ còn {product.StockQuantity}.");

                    unitPrice = product.Price;
                    description = product.Name;

                    // Trừ tồn kho
                    product.StockQuantity -= detailDto.Quantity;
                    _context.Products.Update(product);

                    // ⭐⭐⭐ THÊM MỚI - F7.2b: GHI LOG XUẤT KHO ⭐⭐⭐
                    var inventoryLog = new InventoryTransaction
                    {
                        ProductId = product.Id,
                        ChangeQuantity = -detailDto.Quantity,
                        TransactionType = "Sale",
                        ReferenceType = "Invoice",
                        ReferenceId = invoice.Id,
                        TransactionDate = DateTime.UtcNow,
                        UnitPrice = unitPrice,
                        Notes = $"Bán hàng - Hóa đơn #{invoice.Id} - Khách hàng: {customer.FullName}",
                        CreatedByUserId = userId
                    };
                    _context.InventoryTransactions.Add(inventoryLog);
                    // ⭐⭐⭐ HẾT PHẦN THÊM MỚI ⭐⭐⭐

                    invoiceDetail = new InvoiceDetail
                    {
                        InvoiceId = invoice.Id,
                        ProductId = detailDto.ProductId,
                        Quantity = detailDto.Quantity,
                        UnitPrice = unitPrice,
                        Description = description,
                        SubTotal = detailDto.Quantity * unitPrice
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

    // GET: api/invoices
    [Authorize(Roles = "Admin,Staff,Customer")]
    [HttpGet]
    public async Task<IActionResult> GetInvoices([FromQuery] string? userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = _context.Invoices
            .Include(i => i.User)
            .OrderByDescending(i => i.InvoiceDate)
            .AsQueryable();

        // If user is Customer, always filter by their own userId
        if (User.IsInRole("Customer"))
        {
            query = query.Where(i => i.UserId == currentUserId);
        }
        else if (!string.IsNullOrEmpty(userId))
        {
            // Admin/Staff can filter by userId parameter
            query = query.Where(i => i.UserId == userId);
        }

        var totalRecords = await query.CountAsync();
        var invoices = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new
            {
                i.Id,
                i.InvoiceDate,
                i.TotalAmount,
                i.Status,
                CustomerName = i.User.FullName,
                CustomerId = i.UserId
            })
            .ToListAsync();

        return Ok(new
        {
            Data = invoices,
            TotalRecords = totalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    // GET: api/invoices/{id}
    [Authorize(Roles = "Admin,Staff,Customer")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.User)
            .Include(i => i.InvoiceDetails)
            .ThenInclude(d => d.Product)
            .Include(i => i.InvoiceDetails)
            .ThenInclude(d => d.Service)
            .Where(i => i.Id == id)
            .Select(i => new
            {
                i.Id,
                i.InvoiceDate,
                i.TotalAmount,
                i.Status,
                i.UserId,
                Customer = new
                {
                    Id = i.User.Id,
                    Name = i.User.FullName,
                    Phone = i.User.PhoneNumber
                },
                Details = i.InvoiceDetails.Select(d => new
                {
                    d.Description,
                    d.Quantity,
                    d.UnitPrice,
                    d.SubTotal,
                    ItemName = d.Service != null ? d.Service.Name : (d.Product != null ? d.Product.Name : "")
                })
            })
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            return NotFound(new { Message = "Không tìm thấy hóa đơn." });
        }

        // Check ownership if user is Customer
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (User.IsInRole("Customer") && invoice.UserId != currentUserId)
            return Forbid();

        return Ok(invoice);
    }

    // PUT: api/invoices/{id}/status
    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateInvoiceStatus(int id, [FromBody] InvoiceStatusUpdateDto statusDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
        {
            return NotFound(new { Message = "Không tìm thấy hóa đơn." });
        }
        var allowedStatuses = new[] { "Paid", "Unpaid", "Cancelled" };
        if (!allowedStatuses.Contains(statusDto.Status))
        {
            return BadRequest(new { Message = $"Trạng thái '{statusDto.Status}' không hợp lệ." });
        }

        invoice.Status = statusDto.Status;
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Cập nhật trạng thái hóa đơn thành '{statusDto.Status}' thành công." });
    }
}