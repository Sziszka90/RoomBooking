using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Application.Dtos.BookingDtos;

public record SwapBookingDto
{
    [Required]
    public int ExistingBookingId { get; set; }

    [Required]
    public int NewRoomId { get; set; }
}
