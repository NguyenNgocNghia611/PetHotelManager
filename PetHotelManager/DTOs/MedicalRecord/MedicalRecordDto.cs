using System;
using System.Collections.Generic;
using PetHotelManager.DTOs.Prescription;

namespace PetHotelManager.DTOs.MedicalRecord
{
    public class MedicalRecordDto
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public DateTime ExaminationDate { get; set; }
        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; }
        public string? VeterinarianId { get; set; }
        public string? VeterinarianName { get; set; }
        public List<PrescriptionDetailDto>? Prescriptions { get; set; }
    }
}