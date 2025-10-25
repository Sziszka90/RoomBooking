using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Application.Dtos.BookingDtos;

public record SwapBookingRequest
{
    [Required]
    public int ExistingBookingId { get; set; }

    [Required]
    public int NewRoomId { get; set; }
}
