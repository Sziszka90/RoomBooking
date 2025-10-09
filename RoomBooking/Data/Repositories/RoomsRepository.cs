using Microsoft.EntityFrameworkCore;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Models;

namespace RoomBooking.Data.Repositories;

public class RoomsRepository : IRoomsRepository
{
    private readonly ApplicationDbContext _db;

    public RoomsRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Room>> GetAllAsync() => await _db.Rooms.ToListAsync();

    public async Task<Room?> GetByIdAsync(int id) => await _db.Rooms.FindAsync(id);

    public async Task<Room> AddAsync(Room room)
    {
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
        return room;
    }

    public async Task<bool> ExistsAsync(int id) => await _db.Rooms.AnyAsync(r => r.Id == id);

    public async Task<List<Room>> GetAvailableRoomsAsync(DateTimeOffset start, DateTimeOffset end)
    {
        var roomsWithBookings = await _db.Rooms
            .Include(r => r.Bookings)
            .ToListAsync();

        var availableRooms = roomsWithBookings
            .Where(r => !r.Bookings.Any(b => !(b.End <= start || b.Start >= end)))
            .ToList();

        return availableRooms;
    }
}
