using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Application.Dtos.RoomDtos;

public record CreateRoomDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal PricePerDay { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Address { get; set; } = "Budapest";
}
