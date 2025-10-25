

using RoomBooking.Application.Dtos.BookingDtos;

namespace RoomBooking.Application.Services;

public interface IBookingsService
{
    Task<BookingResponse> CreateAsync(CreateBookingRequest input);
    Task<BookingResponse> GetByIdAsync(int id);
    Task<List<BookingResponse>> GetBookingForRoomAsync(int roomId);
    Task<List<BookingResponse>> GetUserHistoryAsync(
        string booker,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        decimal? minPrice = null,
        decimal? maxPrice = null);
    Task PrintUserHistoryAsync(string booker);
    Task CancelAsync(int id);
    Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end);
    Task<BookingResponse> SwapAsync(SwapBookingRequest swapBookingDto);
}
