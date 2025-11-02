namespace PetHotelManager.DTOs.Rooms
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string RoomTypeName { get; set; }
        public decimal PricePerDay { get; set; }
        public string Status { get; set; }
    }

    public class CreateRoomDto
    {
        public string RoomNumber { get; set; }
        public int RoomTypeId { get; set; }
        public string Status { get; set; }
    }

    public class UpdateRoomDto : CreateRoomDto
    {
        public int Id { get; set; }
    }
}
