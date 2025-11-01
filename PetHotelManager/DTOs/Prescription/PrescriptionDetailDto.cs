namespace PetHotelManager.DTOs.Prescription
{
    public class PrescriptionDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public string? Dosage { get; set; }
    }
}