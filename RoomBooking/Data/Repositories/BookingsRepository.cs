using Microsoft.EntityFrameworkCore;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Models;

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

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Successfully created booking with ID {BookingId}", booking.Id);
        return booking;
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

    public async Task<List<Booking>> GetForRoomAsync(int roomId)
    {
        _logger.LogInformation("Retrieving all bookings for room {RoomId}", roomId);
        var bookings = await _db.Bookings.Include(b => b.Room).Where(b => b.RoomId == roomId).ToListAsync();
        var sortedBookings = bookings.OrderByDescending(b => b.Start).ToList();

        _logger.LogInformation("Found {BookingCount} bookings for room {RoomId}", sortedBookings.Count, roomId);
        return sortedBookings;
    }

    public async Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end)
    {
        _logger.LogInformation("Checking for booking overlaps in room {RoomId} between {Start} and {End}",
            roomId, start, end);

        var roomBookings = await _db.Bookings.Where(b => b.RoomId == roomId).ToListAsync();
        var hasOverlap = roomBookings.Any(b => !(b.End <= start || b.Start >= end));

        _logger.LogInformation("Overlap check for room {RoomId}: {HasOverlap} (checked {BookingCount} bookings)",
            roomId, hasOverlap, roomBookings.Count);

        return hasOverlap;
    }

    public async Task RemoveAsync(Booking booking)
    {
        _logger.LogInformation("Removing booking {BookingId} for room {RoomId}", booking.Id, booking.RoomId);
        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Successfully removed booking {BookingId}", booking.Id);
    }
}
