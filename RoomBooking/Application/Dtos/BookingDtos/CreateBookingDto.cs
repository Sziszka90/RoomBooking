using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Application.Dtos.BookingDtos;

public record CreateBookingDto
{
    [Required]
    public int RoomId { get; set; }

    [Required]
    public DateTimeOffset Start { get; set; }

    [Required]
    public DateTimeOffset End { get; set; }

    [Required]
    [MaxLength(200)]
    public string Booker { get; set; } = string.Empty;
}

