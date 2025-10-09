
using RoomBooking.Application.Dtos.RoomDtos;

namespace RoomBooking.Application.Services;

public interface IRoomsService
{
    Task<List<RoomDto>> GetAllAsync();
    Task<RoomDto> GetByIdAsync(int id);
    Task<RoomDto> CreateAsync(CreateRoomDto createRoomDto);
    Task<List<RoomDto>> GetAvailableRooms(DateTimeOffset start, DateTimeOffset end);
    Task DeleteAsync(int id);
}
