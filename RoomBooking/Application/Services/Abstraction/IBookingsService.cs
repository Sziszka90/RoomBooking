

using RoomBooking.Application.Dtos.BookingDtos;

namespace RoomBooking.Application.Services;

public interface IBookingsService
{
    Task<BookingDto> CreateAsync(CreateBookingDto input);
    Task<BookingDto> GetByIdAsync(int id);
    Task<List<BookingDto>> GetBookingForRoomAsync(int roomId);
    Task<List<BookingDto>> GetUserHistoryAsync(
        string booker,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        decimal? minPrice = null,
        decimal? maxPrice = null);
    Task PrintUserHistoryAsync(string booker);
    Task CancelAsync(int id);
    Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end);
    Task<BookingDto> SwapAsync(SwapBookingDto swapBookingDto);
}
