using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Product;
using PetHotelManager.Models;
using System.Security.Claims;

namespace PetHotelManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/inventory/receive
        [HttpPost("receive")]
        public async Task<IActionResult> Receive([FromBody] InventoryReceiptDto dto)
        {
            if (dto == null || dto.Lines == null || !dto.Lines.Any())
                return BadRequest(new { Message = "Phiếu nhập rỗng." });

            // get user id from token (nullable)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var line in dto.Lines)
                {
                    if (line.Quantity <= 0)
                    {
                        return BadRequest(new { Message = $"Quantity phải > 0 cho productId={line.ProductId}." });
                    }

                    var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == line.ProductId);
                    if (product == null)
                    {
                        return BadRequest(new { Message = $"ProductId {line.ProductId} không tồn tại." });
                    }

                    // Cập nhật tồn (Product model hiện có có StockQuantity)
                    product.StockQuantity += line.Quantity;
                    _context.Products.Update(product);

                    // Ghi inventory transaction
                    var inv = new InventoryTransaction
                    {
                        ProductId = line.ProductId,
                        ChangeQuantity = line.Quantity,
                        TransactionType = "Receipt",
                        ReferenceId = null,
                        Notes = line.Notes ?? dto.Supplier,
                        CreatedByUserId = userId
                    };
                    _context.InventoryTransactions.Add(inv);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new { Status = "Success", Message = "Nhập kho thành công." });
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                return StatusCode(409, new { Message = "Xung đột dữ liệu khi cập nhật kho, vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { Message = "Lỗi khi nhập kho.", Detail = ex.Message });
            }
        }
    }
}