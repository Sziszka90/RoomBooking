using RoomBooking.Domain;

namespace RoomBooking.Data.Repositories.Abstraction;

public interface IBookingsRepository
{
    Task<Booking> AddAsync(Booking booking);
    Task<Booking?> GetByIdAsync(int id);
    Task<List<Booking>> GetBookingForRoomAsync(int roomId);
    Task<List<Booking>> GetUserHistoryAsync(
        string booker,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        decimal? minPrice = null,
        decimal? maxPrice = null);
    Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end);
    Task<Booking> UpdateAsync(Booking booking);
}
