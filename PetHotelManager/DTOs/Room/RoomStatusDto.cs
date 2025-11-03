namespace PetHotelManager.DTOs.Rooms
{
    public class RoomStatusDto
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public string Status { get; set; } // Available, Occupied, Maintenance

        public int? CurrentAppointmentId { get; set; }
        public string? PetName { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
    }
}
