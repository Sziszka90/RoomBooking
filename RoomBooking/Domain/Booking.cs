using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Models;

public class Booking
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Room ID must be a positive number")]
    public int RoomId { get; set; }

    public Room? Room { get; set; }

    [Required]
    public DateTimeOffset Start { get; set; }

    [Required]
    public DateTimeOffset End { get; set; }

    [Required]
    [MaxLength(200, ErrorMessage = "Booker name cannot exceed 200 characters")]
    [MinLength(1, ErrorMessage = "Booker name is required")]
    public string Booker { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0")]
    public decimal TotalPrice { get; set; }

    [Required]
    public DateTimeOffset BookingDate { get; set; }

    public int NumberOfDays => Math.Max(1, (int)(End.Date - Start.Date).TotalDays);

    [Required]
    public bool IsCancelled { get; set; } = false;
}
