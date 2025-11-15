using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.Product;
using PetHotelManager.Models;

namespace PetHotelManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff,Veterinarian")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string? search, [FromQuery] string? category)
        {
            var query = _context.Products.AsQueryable();

            // Filter by search term
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var products = await query
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Unit = p.Unit,
                    MinimumStock = p.MinimumStock,
                    ReorderLevel = p.ReorderLevel,
                    Category = p.Category,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new { Message = "Không tìm thấy sản phẩm" });

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Unit = product.Unit,
                MinimumStock = product.MinimumStock,
                ReorderLevel = product.ReorderLevel,
                Category = product.Category,
                IsActive = product.IsActive
            };

            return Ok(dto);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Unit = dto.Unit,
                MinimumStock = dto.MinimumStock,
                ReorderLevel = dto.ReorderLevel,
                Category = dto.Category,
                IsActive = dto.IsActive
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var resultDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Unit = product.Unit,
                MinimumStock = product.MinimumStock,
                ReorderLevel = product.ReorderLevel,
                Category = product.Category,
                IsActive = product.IsActive
            };

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, resultDto);
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { Message = "ID không khớp" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { Message = "Không tìm thấy sản phẩm" });

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.Unit = dto.Unit;
            product.MinimumStock = dto.MinimumStock;
            product.ReorderLevel = dto.ReorderLevel;
            product.Category = dto.Category;
            product.IsActive = dto.IsActive;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { Message = "Không tìm thấy sản phẩm" });

            // Soft delete: Chỉ set IsActive = false
            product.IsActive = false;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}