using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Application.Dtos.BookingDtos;

public class CreateBookingDto
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

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0")]
    public decimal TotalPrice { get; set; }
}

