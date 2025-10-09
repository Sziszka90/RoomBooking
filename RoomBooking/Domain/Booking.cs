using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Models;

public class Booking
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RoomId { get; set; }

    public Room? Room { get; set; }

    [Required]
    public DateTimeOffset Start { get; set; }

    [Required]
    public DateTimeOffset End { get; set; }

    [Required]
    [MaxLength(200)]
    public string Booker { get; set; } = string.Empty;
}
