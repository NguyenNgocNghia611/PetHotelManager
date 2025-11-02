using System.ComponentModel.DataAnnotations;

namespace PetHotelManager.DTOs.Pets;

public class PetCreateDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(50)]
    public string? Species { get; set; }

    [StringLength(50)]
    public string? Breed { get; set; }
}
