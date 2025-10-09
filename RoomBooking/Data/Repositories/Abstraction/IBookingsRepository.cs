using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Models;

namespace RoomBooking.Data.Repositories.Abstraction;

public interface IBookingsRepository
{
    Task<Booking> AddAsync(Booking booking);
    Task<Booking?> GetByIdAsync(int id);
    Task<List<Booking>> GetForRoomAsync(int roomId);
    Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end);
    Task RemoveAsync(Booking booking);
}
