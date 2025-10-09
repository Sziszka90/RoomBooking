using RoomBooking.Models;

namespace RoomBooking.Data.Repositories.Abstraction;

public interface IRoomsRepository
{
    Task<List<Room>> GetAllAsync();
    Task<Room?> GetByIdAsync(int id);
    Task<Room> AddAsync(Room room);
    Task<bool> ExistsAsync(int id);
    Task<List<Room>> GetAvailableRoomsAsync(DateTimeOffset start, DateTimeOffset end);
    Task RemoveAsync(Room room);
}
