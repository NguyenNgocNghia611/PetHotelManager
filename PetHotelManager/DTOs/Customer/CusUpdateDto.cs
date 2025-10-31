namespace PetHotelManager.DTOs.Customer
{
    public class CusUpdateDto
    {
        public string FullName { get; set; } 
        public string Email { get; set; } 
        public string PhoneNumber { get; set; } 
        public bool IsActive { get; set; } = true;
    }
}
