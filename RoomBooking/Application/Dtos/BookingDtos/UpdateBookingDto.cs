using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Application.Dtos.BookingDtos;

public class UpdateBookingDto
{
    [Required]
    public DateTimeOffset Start { get; set; }

    [Required]
    public DateTimeOffset End { get; set; }

    [Required]
    [MaxLength(200)]
    public string Booker { get; set; } = string.Empty;
}
