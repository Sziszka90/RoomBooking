
using RoomBooking.Application.Dtos.RoomDtos;

namespace RoomBooking.Application.Services;

public interface IRoomsService
{
    Task<List<RoomResponse>> GetAllAsync();
    Task<RoomResponse> GetByIdAsync(int id);
    Task<RoomResponse> CreateAsync(CreateRoomRequest createRoomDto);
    Task<List<RoomResponse>> GetAvailableRooms(
        DateTimeOffset start,
        DateTimeOffset end,
        decimal? minPrice,
        decimal? maxPrice);
    Task DeleteAsync(int id);
}
