using Microsoft.EntityFrameworkCore;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Models;

namespace RoomBooking.Data.Repositories;

public class BookingsRepository : IBookingsRepository
{
    private readonly ApplicationDbContext _db;

    public BookingsRepository(ApplicationDbContext db) => _db = db;

    public async Task<Booking> AddAsync(Booking booking)
    {
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> GetByIdAsync(int id) => await _db.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);

    public async Task<List<Booking>> GetForRoomAsync(int roomId)
    {
        var bookings = await _db.Bookings.Where(b => b.RoomId == roomId).ToListAsync();
        return bookings.OrderByDescending(b => b.Start).ToList();
    }

    public async Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end)
    {
        var roomBookings = await _db.Bookings.Where(b => b.RoomId == roomId).ToListAsync();
        return roomBookings.Any(b => !(b.End <= start || b.Start >= end));
    }

    public async Task RemoveAsync(Booking booking)
    {
        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();
    }
}
