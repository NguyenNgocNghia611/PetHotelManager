namespace PetHotelManager.DTOs.Pets
{
    public class PetDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public int Age { get; set; }
        public string? Color { get; set; }
        public string? HealthStatus { get; set; }
        public string? ImageUrl { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerPhone { get; set; }
    }
}
