

using RoomBooking.Application.Dtos.BookingDtos;

namespace RoomBooking.Application.Services;

public interface IBookingsService
{
    Task<BookingDto> CreateAsync(CreateBookingDto input);
    Task<BookingDto> GetByIdAsync(int id);
    Task<List<BookingDto>> GetForRoomAsync(int roomId);
    Task CancelAsync(int id);
    Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end);
}
