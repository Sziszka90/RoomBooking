using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RoomBooking.Models;

public class Room
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [JsonIgnore]
    public List<Booking> Bookings { get; set; } = new();
}
