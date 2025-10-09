using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Application.Dtos.RoomDtos;

public class UpdateRoomDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}
