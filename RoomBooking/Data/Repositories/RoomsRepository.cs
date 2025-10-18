using Microsoft.EntityFrameworkCore;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Domain;

namespace RoomBooking.Data.Repositories;

public class RoomsRepository : IRoomsRepository
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<RoomsRepository> _logger;

    public RoomsRepository(ApplicationDbContext db, ILogger<RoomsRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<Room>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all rooms");
        var rooms = await _db.Rooms.ToListAsync();
        _logger.LogInformation("Retrieved {RoomCount} rooms", rooms.Count);
        return rooms;
    }

    public async Task<Room?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving room with ID {RoomId}", id);
        var room = await _db.Rooms.FindAsync(id);

        if (room == null)
        {
            _logger.LogWarning("Room with ID {RoomId} not found", id);
        }
        else
        {
            _logger.LogInformation("Successfully retrieved room {RoomId}: {RoomName}", room.Id, room.Name);
        }

        return room;
    }

    public async Task<Room> AddAsync(Room room)
    {
        _logger.LogInformation("Creating new room: {RoomName} with capacity {Capacity}", room.Name, room.Capacity);
        var result = await _db.Rooms.AddAsync(room);
        _logger.LogInformation("Successfully created room with ID {RoomId}: {RoomName}", room.Id, room.Name);
        return result.Entity;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        _logger.LogInformation("Checking if room {RoomId} exists", id);
        var exists = await _db.Rooms.AnyAsync(r => r.Id == id);
        _logger.LogInformation("Room {RoomId} exists: {Exists}", id, exists);
        return exists;
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        _logger.LogInformation("Finding available rooms between {Start} and {End}", start, end);

        var query = _db.Rooms.AsQueryable();

        if (minPrice != null)
        {
            query = query.Where(r => r.PricePerDay >= minPrice);
        }
        if (maxPrice != null)
        {
            query = query.Where(r => r.PricePerDay <= maxPrice);
        }

        var roomsWithBookings = await query
            .Include(r => r.Bookings)
            .ToListAsync();

        _logger.LogInformation("Loaded {TotalRooms} rooms with their bookings", roomsWithBookings.Count);

        var availableRooms = roomsWithBookings
            .Where(r => !r.Bookings.Any(b => !(b.End <= start || b.Start >= end)))
            .ToList();

        _logger.LogInformation("Found {AvailableRooms} available rooms out of {TotalRooms} total rooms",
            availableRooms.Count, roomsWithBookings.Count);

        return availableRooms;
    }

    public void Remove(Room room)
    {
        _logger.LogInformation("Removing room {RoomId}: {RoomName}", room.Id, room.Name);
        _db.Rooms.Remove(room);
        _logger.LogInformation("Successfully removed room {RoomId}: {RoomName}", room.Id, room.Name);
    }
}
