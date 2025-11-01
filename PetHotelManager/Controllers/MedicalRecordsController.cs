﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Data;
using PetHotelManager.DTOs.MedicalRecord;
using PetHotelManager.DTOs.Prescription;
using PetHotelManager.Models;
using System.Security.Claims;

namespace PetHotelManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/MedicalRecords
        // Cho phép Staff, Doctor, Veterinarian (tương thích seed/roles hiện tại)
        [Authorize(Roles = "Staff,Doctor,Veterinarian")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Kiểm tra pet tồn tại
            var pet = await _context.Pets.FindAsync(dto.PetId);
            if (pet == null) return BadRequest(new { Message = "Pet không tồn tại." });

            // Transaction để đảm bảo atomicity (medical + prescriptions + trừ kho)
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var medical = new MedicalRecord
                {
                    PetId = dto.PetId,
                    ExaminationDate = dto.ExaminationDate,
                    Symptoms = dto.Symptoms ?? string.Empty,
                    Diagnosis = dto.Diagnosis ?? string.Empty,
                    VeterinarianId = userId
                };

                _context.MedicalRecords.Add(medical);
                await _context.SaveChangesAsync(); // để có medical.Id

                if (dto.Prescriptions != null && dto.Prescriptions.Any())
                {
                    foreach (var p in dto.Prescriptions)
                    {
                        var product = await _context.Products.FindAsync(p.ProductId);
                        if (product == null)
                            throw new InvalidOperationException($"ProductId {p.ProductId} không tồn tại.");

                        // Repo hiện dùng StockQuantity (xem ProductsController). Nếu tên khác, chỉnh ở đây.
                        var stockProp = product.GetType().GetProperty("StockQuantity");
                        if (stockProp != null)
                        {
                            var stockVal = (int)stockProp.GetValue(product)!;
                            if (stockVal < p.Quantity)
                                throw new InvalidOperationException($"Không đủ tồn kho cho productId={p.ProductId} (còn {stockVal}).");

                            // trừ kho
                            stockProp.SetValue(product, stockVal - p.Quantity);
                            _context.Products.Update(product);
                        }

                        var pd = new PrescriptionDetail
                        {
                            MedicalRecordId = medical.Id,
                            ProductId = p.ProductId,
                            Quantity = p.Quantity,
                            Dosage = p.Dosage ?? string.Empty
                        };
                        _context.PrescriptionDetails.Add(pd);
                    }
                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();

                return Ok(new { Status = "Success", MedicalRecordId = medical.Id });
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync();
                return BadRequest(new { Message = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                return StatusCode(409, new { Message = "Xung đột dữ liệu, vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { Message = "Lỗi server", Detail = ex.Message });
            }
        }

        // GET: api/MedicalRecords/pet/{petId}
        [Authorize]
        [HttpGet("pet/{petId}")]
        public async Task<IActionResult> GetByPet(int petId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var records = await _context.MedicalRecords
                .AsNoTracking()
                .Include(m => m.PrescriptionDetails).ThenInclude(pd => pd.Product)
                .Include(m => m.Veterinarian)
                .Include(m => m.Pet)
                .Where(m => m.PetId == petId)
                .OrderByDescending(m => m.ExaminationDate)
                .ToListAsync();

            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff") || User.IsInRole("Doctor") || User.IsInRole("Veterinarian");

            if (!isPrivileged)
            {
                var pet = await _context.Pets.FindAsync(petId);
                if (pet == null) return NotFound();
                if (pet.UserId != userId) return Forbid();
            }

            var result = records.Select(m => new MedicalRecordDto
            {
                Id = m.Id,
                PetId = m.PetId,
                ExaminationDate = m.ExaminationDate,
                Symptoms = m.Symptoms,
                Diagnosis = m.Diagnosis,
                VeterinarianId = m.VeterinarianId,
                VeterinarianName = m.Veterinarian?.FullName,
                Prescriptions = m.PrescriptionDetails?.Select(pd => new PrescriptionDetailDto
                {
                    Id = pd.Id,
                    ProductId = pd.ProductId,
                    ProductName = pd.Product?.Name,
                    Quantity = pd.Quantity,
                    Dosage = pd.Dosage
                }).ToList()
            });

            return Ok(result);
        }

        // GET: api/MedicalRecords/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var m = await _context.MedicalRecords
                .AsNoTracking()
                .Include(x => x.PrescriptionDetails).ThenInclude(pd => pd.Product)
                .Include(x => x.Pet)
                .Include(x => x.Veterinarian)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (m == null) return NotFound();

            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("Staff") || User.IsInRole("Doctor") || User.IsInRole("Veterinarian");
            if (!isPrivileged && m.Pet.UserId != userId) return Forbid();

            var dto = new MedicalRecordDto
            {
                Id = m.Id,
                PetId = m.PetId,
                ExaminationDate = m.ExaminationDate,
                Symptoms = m.Symptoms,
                Diagnosis = m.Diagnosis,
                VeterinarianId = m.VeterinarianId,
                VeterinarianName = m.Veterinarian?.FullName,
                Prescriptions = m.PrescriptionDetails?.Select(pd => new PrescriptionDetailDto
                {
                    Id = pd.Id,
                    ProductId = pd.ProductId,
                    ProductName = pd.Product?.Name,
                    Quantity = pd.Quantity,
                    Dosage = pd.Dosage
                }).ToList()
            };

            return Ok(dto);
        }
    }
}