namespace RoomBooking.Application.Dtos.RoomDtos;

public record RoomResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal PricePerDay { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}
