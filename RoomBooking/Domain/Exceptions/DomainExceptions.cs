namespace RoomBooking.Domain.Exceptions;

public class InvalidBookingException : ArgumentException
{
    public InvalidBookingException(string message) : base(message) { }
}

public class ValidationException : ArgumentException
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}

public class OverlapException : InvalidOperationException
{
    public OverlapException(string message) : base(message) { }
    public OverlapException(int roomId, DateTimeOffset start, DateTimeOffset end)
        : base($"Room {roomId} is already booked for the time period from {start:yyyy-MM-dd HH:mm} to {end:yyyy-MM-dd HH:mm}") { }
}

public class RoomNotFoundException : KeyNotFoundException
{
    public RoomNotFoundException(int roomId) : base($"Room {roomId} not found") { }
}

public class BookingNotFoundException : KeyNotFoundException
{
    public BookingNotFoundException(int bookingId) : base($"Booking {bookingId} not found") { }
}

public class BookingConflictException : InvalidOperationException
{
    public BookingConflictException(string message) : base(message) { }
}

public class RoomDeletionException : InvalidOperationException
{
    public RoomDeletionException(string message) : base(message) { }
}
