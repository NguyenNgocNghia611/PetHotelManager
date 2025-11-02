namespace PetHotelManager.DTOs.Appointments
{
    public class AppointmentListDto
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? PetName { get; set; }
        public string? ServiceName { get; set; }
        public string? RoomName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; }
    }
}
