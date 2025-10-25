namespace PetHotelManager.DTOs.Auth;

public class LoginResponseDto
{
    public UserDto User  { get; set; }
    public string  Token { get; set; }
}