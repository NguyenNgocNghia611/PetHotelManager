namespace PetHotelManager.DTOs.RoomTypes
{
    public class RoomTypeDto
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public decimal PricePerDay { get; set; }
        public string? Description { get; set; }
    }

    public class CreateRoomTypeDto
    {
        public string TypeName { get; set; }
        public decimal PricePerDay { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateRoomTypeDto : CreateRoomTypeDto
    {
        public int Id { get; set; }
    }
}
