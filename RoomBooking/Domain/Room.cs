using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Domain;

public class Room
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal PricePerDay { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Address { get; set; } = "Budapest";

    public List<Booking> Bookings { get; set; } = new();
}
