using RoomBooking.Data.Repositories.Abstraction;

namespace RoomBooking.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    private IBookingsRepository? _bookings;
    private IRoomsRepository? _rooms;

    public UnitOfWork(
        ApplicationDbContext context,
        ILogger<UnitOfWork> logger,
        ILogger<BookingsRepository> bookingsLogger,
        ILogger<RoomsRepository> roomsLogger)
    {
        _context = context;
        _logger = logger;
        _bookingsLogger = bookingsLogger;
        _roomsLogger = roomsLogger;
    }

    private readonly ILogger<BookingsRepository> _bookingsLogger;
    private readonly ILogger<RoomsRepository> _roomsLogger;

    public IBookingsRepository Bookings
    {
        get
        {
            _bookings ??= new BookingsRepository(_context, _bookingsLogger);
            return _bookings;
        }
    }

    public IRoomsRepository Rooms
    {
        get
        {
            _rooms ??= new RoomsRepository(_context, _roomsLogger);
            return _rooms;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        _logger.LogInformation("Saving changes through Unit of Work");
        var result = await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully saved {ChangeCount} changes", result);
        return result;
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing Unit of Work");
        _context.Dispose();
        _logger.LogInformation("Unit of Work disposed");
    }
}
