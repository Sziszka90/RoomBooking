namespace RoomBooking.Domain.Exceptions;

public class InvalidBookingException : ArgumentException
{
    public InvalidBookingException(string message) : base(message) { }
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
