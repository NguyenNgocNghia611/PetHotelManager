namespace PetHotelManager.DTOs.Pets
{
    public class PetListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Species { get; set; }
        public string? ImageUrl { get; set; }
        public string? OwnerName { get; set; }
    }
}
