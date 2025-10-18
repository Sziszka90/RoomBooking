
using RoomBooking.Application.Dtos.RoomDtos;

namespace RoomBooking.Application.Dtos.BookingDtos;

public record BookingDto
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public DateTimeOffset Start { get; set; }

    public DateTimeOffset End { get; set; }

    public string Booker { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }

    public DateTimeOffset BookingDate { get; set; }

    public int NumberOfDays { get; set; }

    public RoomDto? Room { get; set; }

    public bool IsCancelled { get; set; }
}
