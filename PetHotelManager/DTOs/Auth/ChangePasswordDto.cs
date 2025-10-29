using System.ComponentModel.DataAnnotations;

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; }

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; }
}