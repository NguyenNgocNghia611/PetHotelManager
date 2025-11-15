using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Inventory;
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

        // ===== F7.1 - NHẬP KHO =====

        /// <summary>
        /// F7.1 - Tạo phiếu nhập kho
        /// POST /api/inventory/receive
        /// </summary>
        [HttpPost("receive")]
        public async Task<IActionResult> Receive([FromBody] InventoryReceiptDto receiptDto)  // ⭐ ĐỔI TÊN
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var receiptDate = receiptDto.ReceiptDate ?? DateTime.UtcNow;  // ⭐ ĐỔI TÊN

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var line in receiptDto.Lines)  // ⭐ ĐỔI TÊN
                {
                    // 1. Lấy sản phẩm
                    var product = await _context.Products.FindAsync(line.ProductId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { Message = $"Sản phẩm ID {line.ProductId} không tồn tại" });
                    }

                    // 2. Tăng tồn kho
                    product.StockQuantity += line.Quantity;
                    _context.Products.Update(product);

                    // 3. Ghi log giao dịch
                    var inventoryLog = new InventoryTransaction
                    {
                        ProductId = line.ProductId,
                        ChangeQuantity = line.Quantity,
                        TransactionType = "Receipt",
                        ReferenceType = "Manual",
                        ReferenceId = null,
                        Supplier = receiptDto.Supplier,  // ⭐ ĐỔI TÊN
                        UnitPrice = line.UnitPrice,
                        Notes = line.Notes ?? receiptDto.Notes,  // ⭐ ĐỔI TÊN
                        TransactionDate = receiptDate,
                        CreatedByUserId = userId
                    };
                    _context.InventoryTransactions.Add(inventoryLog);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Nhập kho thành công",
                    TotalProducts = receiptDto.Lines.Count,  // ⭐ ĐỔI TÊN
                    TotalQuantity = receiptDto.Lines.Sum(l => l.Quantity)  // ⭐ ĐỔI TÊN
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "Lỗi khi nhập kho", Error = ex.Message });
            }
        }

        // ===== F7.2c - XUẤT KHO THỦ CÔNG =====

        /// <summary>
        /// F7.2c - Xuất kho thủ công (hàng hỏng/hết hạn/mất)
        /// POST /api/inventory/issue
        /// </summary>
        [HttpPost("issue")]
        public async Task<IActionResult> Issue([FromBody] InventoryIssueDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var line in dto.Lines)
                {
                    // 1. Lấy sản phẩm
                    var product = await _context.Products.FindAsync(line.ProductId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { Message = $"Sản phẩm ID {line.ProductId} không tồn tại" });
                    }

                    // 2. Kiểm tra tồn kho
                    if (product.StockQuantity < line.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new
                        {
                            Message = $"Không đủ tồn kho cho {product.Name}. Còn: {product.StockQuantity}, Yêu cầu: {line.Quantity}"
                        });
                    }

                    // 3. Trừ tồn kho
                    product.StockQuantity -= line.Quantity;
                    _context.Products.Update(product);

                    // 4. Ghi log
                    var inventoryLog = new InventoryTransaction
                    {
                        ProductId = line.ProductId,
                        ChangeQuantity = -line.Quantity,
                        TransactionType = "Issue",
                        ReferenceType = "Manual",
                        ReferenceId = null,
                        Notes = $"Lý do: {dto.Reason}. {line.Notes ?? dto.Notes}",
                        TransactionDate = DateTime.UtcNow,
                        CreatedByUserId = userId
                    };
                    _context.InventoryTransactions.Add(inventoryLog);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Xuất kho thành công",
                    TotalProducts = dto.Lines.Count,
                    TotalQuantity = dto.Lines.Sum(l => l.Quantity)
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "Lỗi khi xuất kho", Error = ex.Message });
            }
        }

        // ===== F7.3 - XEM TỒN KHO =====

        /// <summary>
        /// F7.3 - Xem báo cáo tồn kho
        /// GET /api/inventory/stock
        /// </summary>
        [HttpGet("stock")]
        public async Task<IActionResult> GetStockReport(
            [FromQuery] string? search,
            [FromQuery] string? category)
        {
            var query = _context.Products.AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var products = await query
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new StockReportDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Category = p.Category,
                    CurrentStock = p.StockQuantity,
                    MinimumStock = p.MinimumStock,
                    ReorderLevel = p.ReorderLevel,
                    Unit = p.Unit,
                    Price = p.Price,
                    StockStatus = p.StockQuantity == 0 ? "OutOfStock" :
                                  p.StockQuantity < p.MinimumStock ? "Critical" :
                                  p.StockQuantity < p.ReorderLevel ? "Warning" : "Normal",
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(new
            {
                TotalProducts = products.Count,
                TotalValue = products.Sum(p => p.TotalValue),
                LowStockCount = products.Count(p => p.StockStatus != "Normal"),
                Products = products
            });
        }

        // ===== F7.3 - LỊCH SỬ GIAO DỊCH =====

        /// <summary>
        /// F7.3 - Xem lịch sử giao dịch kho
        /// GET /api/inventory/transactions
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] int? productId,
            [FromQuery] string? transactionType,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.InventoryTransactions
                .Include(t => t.Product)
                .Include(t => t.CreatedByUser)
                .AsQueryable();

            // Filter
            if (productId.HasValue)
            {
                query = query.Where(t => t.ProductId == productId.Value);
            }

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.TransactionType == transactionType);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            // Pagination
            var totalRecords = await query.CountAsync();
            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new InventoryTransactionDto
                {
                    Id = t.Id,
                    ProductId = t.ProductId,
                    ProductName = t.Product.Name,
                    ChangeQuantity = t.ChangeQuantity,
                    TransactionType = t.TransactionType,
                    ReferenceType = t.ReferenceType,
                    ReferenceId = t.ReferenceId,
                    TransactionDate = t.TransactionDate,
                    Supplier = t.Supplier,
                    UnitPrice = t.UnitPrice,
                    Notes = t.Notes,
                    CreatedByUserName = t.CreatedByUser != null ? t.CreatedByUser.FullName : null
                })
                .ToListAsync();

            return Ok(new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Transactions = transactions
            });
        }

        // ===== F7.4 - CẢNH BÁO TỒN KHO =====

        /// <summary>
        /// F7.4 - Lấy danh sách cảnh báo tồn kho thấp
        /// GET /api/inventory/low-stock-alerts
        /// </summary>
        [HttpGet("low-stock-alerts")]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            var alerts = await _context.Products
                .Where(p => p.IsActive && p.StockQuantity <= p.ReorderLevel)
                .OrderBy(p => p.StockQuantity)
                .Select(p => new LowStockAlertDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Category = p.Category,
                    CurrentStock = p.StockQuantity,
                    MinimumStock = p.MinimumStock,
                    ReorderLevel = p.ReorderLevel,
                    Unit = p.Unit,
                    Severity = p.StockQuantity == 0 ? "OutOfStock" :
                               p.StockQuantity < p.MinimumStock ? "Critical" : "Warning",
                    SuggestedOrderQuantity = p.ReorderLevel * 2
                })
                .ToListAsync();

            return Ok(new
            {
                TotalAlerts = alerts.Count,
                OutOfStockCount = alerts.Count(a => a.Severity == "OutOfStock"),
                CriticalCount = alerts.Count(a => a.Severity == "Critical"),
                WarningCount = alerts.Count(a => a.Severity == "Warning"),
                Alerts = alerts
            });
        }

        // ===== F7.4 - THỐNG KÊ KHO =====

        /// <summary>
        /// F7.4 - Thống kê tổng quan kho
        /// GET /api/inventory/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetInventoryStats()
        {
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var stats = new InventoryStatsDto
            {
                TotalProducts = await _context.Products.CountAsync(),
                ActiveProducts = await _context.Products.CountAsync(p => p.IsActive),
                LowStockCount = await _context.Products
                    .CountAsync(p => p.IsActive && p.StockQuantity < p.ReorderLevel && p.StockQuantity >= p.MinimumStock),
                CriticalStockCount = await _context.Products
                    .CountAsync(p => p.IsActive && p.StockQuantity < p.MinimumStock && p.StockQuantity > 0),
                OutOfStockCount = await _context.Products
                    .CountAsync(p => p.IsActive && p.StockQuantity == 0),
                TotalInventoryValue = await _context.Products
                    .Where(p => p.IsActive)
                    .SumAsync(p => p.StockQuantity * p.Price),
                TransactionsToday = await _context.InventoryTransactions
                    .CountAsync(t => t.TransactionDate >= today),
                TransactionsThisMonth = await _context.InventoryTransactions
                    .CountAsync(t => t.TransactionDate >= startOfMonth)
            };

            return Ok(stats);
        }
    }
}