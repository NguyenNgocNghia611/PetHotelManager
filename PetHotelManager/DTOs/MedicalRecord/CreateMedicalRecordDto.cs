using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PetHotelManager.DTOs.Prescription;

namespace PetHotelManager.DTOs.MedicalRecord
{
    public class CreateMedicalRecordDto
    {
        [Required]
        public int PetId { get; set; }

        public string? Symptoms { get; set; }

        public string? Diagnosis { get; set; }

        public DateTime ExaminationDate { get; set; } = DateTime.UtcNow;

        public List<CreatePrescriptionDto>? Prescriptions { get; set; }
    }
}