
namespace RoomBooking.Data.Repositories.Abstraction;

public interface IUnitOfWork : IDisposable
{
    IBookingsRepository Bookings { get; }
    IRoomsRepository Rooms { get; }

    Task<int> SaveChangesAsync();
}
