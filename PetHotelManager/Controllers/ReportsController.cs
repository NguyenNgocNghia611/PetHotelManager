namespace PetHotelManager.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;

[ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("revenue-summary")]
        public async Task<IActionResult> GetRevenueSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new { Message = "Ngày bắt đầu không được lớn hơn ngày kết thúc." });
            }

            var correctedEndDate = endDate.Date.AddDays(1).AddTicks(-1);

            var query = _context.Invoices
                                .Where(i => i.Status == "Paid" &&
                                            i.InvoiceDate >= startDate.Date &&
                                            i.InvoiceDate <= correctedEndDate);

            var totalRevenue = await query.SumAsync(i => i.TotalAmount);
            var numberOfInvoices = await query.CountAsync();

            var dailyRevenue = await query
                                    .GroupBy(i => i.InvoiceDate.Date)
                                    .Select(g => new
                                    {
                                        Date = g.Key,
                                        Revenue = g.Sum(i => i.TotalAmount),
                                        InvoiceCount = g.Count()
                                    })
                                    .OrderBy(d => d.Date)
                                    .ToListAsync();

            return Ok(new
            {
                ReportPeriod = new { StartDate = startDate.Date, EndDate = endDate.Date },
                TotalRevenue = totalRevenue,
                TotalPaidInvoices = numberOfInvoices,
                DailyBreakdown = dailyRevenue
            });
        }
    }