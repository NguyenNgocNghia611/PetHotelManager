public class CreateAppointmentDto
{
    public string UserId { get; set; }  
    public int PetId { get; set; }
    public int? ServiceId { get; set; }
    public int? RoomId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string? Notes { get; set; }
}
