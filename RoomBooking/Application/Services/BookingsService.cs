using AutoMapper;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Domain.Exceptions;
using RoomBooking.Domain;

namespace RoomBooking.Application.Services;

public class BookingsService : IBookingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingsService> _logger;

    public BookingsService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<BookingsService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest createBookingDto)
    {
        _logger.LogInformation("Creating booking for room {RoomId} from {Start} to {End} by {Booker}",
            createBookingDto.RoomId, createBookingDto.Start, createBookingDto.End, createBookingDto.Booker);

        if (createBookingDto.End <= createBookingDto.Start)
        {
            _logger.LogWarning("Invalid booking time range: End {End} is not after Start {Start}",
                createBookingDto.End, createBookingDto.Start);
            throw new ValidationException("End date must be after start date");
        }

        var daysDifference = (createBookingDto.End.Date - createBookingDto.Start.Date).TotalDays;
        if (daysDifference < 1)
        {
            _logger.LogWarning("Invalid booking duration: End date {End} must be at least one day after start date {Start}. Days difference: {DaysDifference}",
                createBookingDto.End, createBookingDto.Start, daysDifference);
            throw new ValidationException("End date must be at least one day after start date");
        }

        var room = await _unitOfWork.Rooms.GetByIdAsync(createBookingDto.RoomId);
        if (room == null)
        {
            _logger.LogWarning("Attempted to book non-existent room {RoomId}", createBookingDto.RoomId);
            throw new RoomNotFoundException(createBookingDto.RoomId);
        }

        var overlap = await _unitOfWork.Bookings.AnyOverlapAsync(createBookingDto.RoomId, createBookingDto.Start, createBookingDto.End);
        if (overlap)
        {
            _logger.LogWarning("Booking conflict detected for room {RoomId} between {Start} and {End}",
                createBookingDto.RoomId, createBookingDto.Start, createBookingDto.End);
            throw new OverlapException(createBookingDto.RoomId, createBookingDto.Start, createBookingDto.End);
        }

        var booking = _mapper.Map<Booking>(createBookingDto);

        booking.TotalPrice = room.PricePerDay * (int)(createBookingDto.End.Date - createBookingDto.Start.Date).TotalDays;
        booking.BookingDate = DateTimeOffset.Now;
        booking.IsCancelled = false;

        var result = await _unitOfWork.Bookings.AddAsync(booking);
        _ = await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Successfully created booking {BookingId} for room {RoomId}", result.Id, result.RoomId);

        return _mapper.Map<BookingResponse>(result);
    }

    public async Task<BookingResponse> GetByIdAsync(int id)
    {
        var result = await _unitOfWork.Bookings.GetByIdAsync(id);
        if (result == null) throw new BookingNotFoundException(id);
        return _mapper.Map<BookingResponse>(result);
    }

    public async Task<List<BookingResponse>> GetBookingForRoomAsync(int roomId)
    {
        var result = await _unitOfWork.Bookings.GetBookingForRoomAsync(roomId);
        if (result == null) throw new RoomNotFoundException(roomId);
        return _mapper.Map<List<BookingResponse>>(result);
    }

    public async Task CancelAsync(int id)
    {
        var result = await _unitOfWork.Bookings.GetByIdAsync(id);
        if (result == null) throw new BookingNotFoundException(id);
        result!.IsCancelled = true;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end)
    {
        var result = await _unitOfWork.Bookings.AnyOverlapAsync(roomId, start, end);
        return result;
    }

    public async Task<BookingResponse> SwapAsync(SwapBookingRequest swapBookingDto)
    {
        _logger.LogInformation("Swapping booking {ExistingBookingId} to room {NewRoomId}",
            swapBookingDto.ExistingBookingId, swapBookingDto.NewRoomId);

        var existingBooking = await _unitOfWork.Bookings.GetByIdAsync(swapBookingDto.ExistingBookingId);
        if (existingBooking == null)
        {
            _logger.LogWarning("Existing booking {BookingId} not found for swap", swapBookingDto.ExistingBookingId);
            throw new BookingNotFoundException(swapBookingDto.ExistingBookingId);
        }

        var newRoom = await _unitOfWork.Rooms.GetByIdAsync(swapBookingDto.NewRoomId);
        if (newRoom == null)
        {
            _logger.LogWarning("New room {RoomId} not found for swap", swapBookingDto.NewRoomId);
            throw new RoomNotFoundException(swapBookingDto.NewRoomId);
        }

        var isNewRoomAvailable = await _unitOfWork.Bookings.AnyOverlapAsync(
            swapBookingDto.NewRoomId,
            existingBooking.Start,
            existingBooking.End);

        if (isNewRoomAvailable)
        {
            _logger.LogWarning("New room {NewRoomId} is not available for the time period {Start} to {End}",
                swapBookingDto.NewRoomId, existingBooking.Start, existingBooking.End);
            throw new OverlapException(swapBookingDto.NewRoomId, existingBooking.Start, existingBooking.End);
        }

        var newBooking = new Booking
        {
            RoomId = swapBookingDto.NewRoomId,
            Start = existingBooking.Start,
            End = existingBooking.End,
            Booker = existingBooking.Booker,
            TotalPrice = newRoom.PricePerDay * (int)(existingBooking.End - existingBooking.Start).TotalDays,
            BookingDate = existingBooking.BookingDate,
        };

        var createdBooking = await _unitOfWork.Bookings.AddAsync(newBooking);

        existingBooking.IsCancelled = true;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully swapped booking {ExistingBookingId} from room {OldRoomId} to room {NewRoomId}. New booking ID: {NewBookingId}",
            swapBookingDto.ExistingBookingId, existingBooking.RoomId, swapBookingDto.NewRoomId, createdBooking.Id);

        return _mapper.Map<BookingResponse>(createdBooking);
    }

    public async Task<List<BookingResponse>> GetUserHistoryAsync(
        string booker,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        _logger.LogInformation("Retrieving booking history for user {Booker} with filters: FromDate={FromDate}, ToDate={ToDate}, MinPrice={MinPrice}, MaxPrice={MaxPrice}",
            booker, fromDate, toDate, minPrice, maxPrice);

        if (string.IsNullOrWhiteSpace(booker))
        {
            _logger.LogWarning("Invalid booker name provided: {Booker}", booker);
            throw new ValidationException("Booker name cannot be empty or null");
        }

        var bookings = await _unitOfWork.Bookings.GetUserHistoryAsync(
            booker, fromDate, toDate, minPrice, maxPrice);

        _logger.LogInformation("Successfully retrieved {BookingCount} bookings for user {Booker}", bookings.Count, booker);
        return _mapper.Map<List<BookingResponse>>(bookings);
    }

    public async Task PrintUserHistoryAsync(string booker)
    {
        _logger.LogInformation("Starting to print booking history for user {Booker}", booker);

        if (string.IsNullOrWhiteSpace(booker))
        {
            _logger.LogWarning("Invalid booker name provided for printing history: {Booker}", booker);
            throw new ValidationException("Booker name cannot be empty or null");
        }

        var bookings = await GetUserHistoryAsync(booker);

        if (!bookings.Any())
        {
            _logger.LogInformation("No booking history found for user {Booker}", booker);
            return;
        }

        _logger.LogInformation("=== BOOKING HISTORY FOR USER: {Booker} ===", booker);
        _logger.LogInformation("Total bookings found: {BookingCount}", bookings.Count);
        _logger.LogInformation("========================================");

        foreach (var booking in bookings)
        {
            _logger.LogInformation("Booking ID: {BookingId} | Room: {RoomId} | Start: {Start:yyyy-MM-dd} | End: {End:yyyy-MM-dd} | " +
                                   "Total Price: ${TotalPrice} | Booked On: {BookingDate:yyyy-MM-dd HH:mm} | Cancelled: {IsCancelled}",
                booking.Id, booking.RoomId, booking.Start, booking.End, booking.TotalPrice, booking.BookingDate, booking.IsCancelled);
        }

        _logger.LogInformation("=== END OF BOOKING HISTORY FOR {Booker} ===", booker);
    }
}
