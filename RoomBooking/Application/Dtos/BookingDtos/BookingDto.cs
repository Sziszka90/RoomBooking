
using RoomBooking.Application.Dtos.RoomDtos;

namespace RoomBooking.Application.Dtos.BookingDtos;

public class BookingDto
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public DateTimeOffset Start { get; set; }

    public DateTimeOffset End { get; set; }

    public string Booker { get; set; } = string.Empty;

    public RoomDto? Room { get; set; }
}
