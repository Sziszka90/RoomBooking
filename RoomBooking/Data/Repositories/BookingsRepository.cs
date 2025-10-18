using Microsoft.EntityFrameworkCore;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Domain;

namespace RoomBooking.Data.Repositories;

public class BookingsRepository : IBookingsRepository
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<BookingsRepository> _logger;

    public BookingsRepository(ApplicationDbContext db, ILogger<BookingsRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Booking> AddAsync(Booking booking)
    {
        _logger.LogInformation("Creating new booking for room {RoomId} from {Start} to {End} by {Booker}",
            booking.RoomId, booking.Start, booking.End, booking.Booker);

        var result = await _db.Bookings.AddAsync(booking);

        _logger.LogInformation("Booking added to context for room {RoomId}", booking.RoomId);
        return result.Entity;
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving booking with ID {BookingId}", id);
        var booking = await _db.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            _logger.LogWarning("Booking with ID {BookingId} not found", id);
        }
        else
        {
            _logger.LogInformation("Successfully retrieved booking {BookingId} for room {RoomName}",
                booking.Id, booking.Room?.Name);
        }

        return booking;
    }

    public async Task<List<Booking>> GetBookingForRoomAsync(int roomId)
    {
        _logger.LogInformation("Retrieving all bookings for room {RoomId}", roomId);
        var bookings = await _db.Bookings.Include(b => b.Room).Where(b => b.RoomId == roomId && !b.IsCancelled).ToListAsync();
        var sortedBookings = bookings.OrderByDescending(b => b.Start).ToList();

        _logger.LogInformation("Found {BookingCount} bookings for room {RoomId}", sortedBookings.Count, roomId);
        return sortedBookings;
    }

    public async Task<List<Booking>> GetUserHistoryAsync(
        string booker,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        _logger.LogInformation("Retrieving booking history for user {Booker} with filters: FromDate={FromDate}, ToDate={ToDate}, MinPrice={MinPrice}, MaxPrice={MaxPrice}",
            booker, fromDate, toDate, minPrice, maxPrice);

        var query = _db.Bookings
            .Include(b => b.Room)
            .Where(b => b.Booker == booker);

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.Start >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.Start <= toDate.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(b => b.TotalPrice >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(b => b.TotalPrice <= maxPrice.Value);
        }

        var bookings = await query
            .ToListAsync();

        bookings = bookings.OrderByDescending(b => b.BookingDate).ThenByDescending(b => b.Start).ToList();

        _logger.LogInformation("Found {BookingCount} bookings in history for user {Booker}", bookings.Count, booker);
        return bookings;
    }

    public async Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end)
    {
        _logger.LogInformation("Checking for booking overlaps in room {RoomId} between {Start} and {End}",
            roomId, start, end);

        var roomBookings = await _db.Bookings.Where(b => b.RoomId == roomId && !b.IsCancelled).ToListAsync();
        var hasOverlap = roomBookings.Any(b => !(b.End <= start || b.Start >= end));

        _logger.LogInformation("Overlap check for room {RoomId}: {HasOverlap} (checked {BookingCount} bookings)",
            roomId, hasOverlap, roomBookings.Count);

        return hasOverlap;
    }

    public Task<Booking> UpdateAsync(Booking booking)
    {
        _logger.LogInformation("Updating booking {BookingId} for room {RoomId}", booking.Id, booking.RoomId);
        _db.Bookings.Update(booking);
        _logger.LogInformation("Booking {BookingId} marked for update", booking.Id);
        return Task.FromResult(booking);
    }
}
