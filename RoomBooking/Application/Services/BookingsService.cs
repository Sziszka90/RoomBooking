using AutoMapper;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Models;

namespace RoomBooking.Application.Services;

public class BookingsService : IBookingsService
{
    private readonly IBookingsRepository _bookingsRepository;
    private readonly IRoomsRepository _roomsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingsService> _logger;

    public BookingsService(
        IBookingsRepository bookingsRepository,
        IRoomsRepository roomsRepository,
        IMapper mapper,
        ILogger<BookingsService> logger)
    {
        _bookingsRepository = bookingsRepository;
        _roomsRepository = roomsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto createBookingDto)
    {
        _logger.LogInformation("Creating booking for room {RoomId} from {Start} to {End} by {Booker}",
            createBookingDto.RoomId, createBookingDto.Start, createBookingDto.End, createBookingDto.Booker);

        if (createBookingDto.End <= createBookingDto.Start)
        {
            _logger.LogWarning("Invalid booking time range: End {End} is not after Start {Start}",
                createBookingDto.End, createBookingDto.Start);
            throw new ArgumentException("End must be after Start");
        }

        var room = await _roomsRepository.GetByIdAsync(createBookingDto.RoomId);
        if (room == null)
        {
            _logger.LogWarning("Attempted to book non-existent room {RoomId}", createBookingDto.RoomId);
            throw new InvalidOperationException("Room does not exist");
        }

        var overlap = await _bookingsRepository.AnyOverlapAsync(createBookingDto.RoomId, createBookingDto.Start, createBookingDto.End);
        if (overlap)
        {
            _logger.LogWarning("Booking conflict detected for room {RoomId} between {Start} and {End}",
                createBookingDto.RoomId, createBookingDto.Start, createBookingDto.End);
            throw new InvalidOperationException("Room is already booked for the requested time range");
        }

        var booking = new Booking
        {
            RoomId = createBookingDto.RoomId,
            Start = createBookingDto.Start,
            End = createBookingDto.End,
            Booker = createBookingDto.Booker
        };

        var result = await _bookingsRepository.AddAsync(booking);
        _logger.LogInformation("Successfully created booking {BookingId} for room {RoomId}", result.Id, result.RoomId);

        return _mapper.Map<BookingDto>(result);
    }

    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var result = await _bookingsRepository.GetByIdAsync(id);
        return _mapper.Map<BookingDto>(result);
    }

    public async Task<List<BookingDto>> GetForRoomAsync(int roomId)
    {
        var result = await _bookingsRepository.GetForRoomAsync(roomId);
        return _mapper.Map<List<BookingDto>>(result);
    }

    public async Task CancelAsync(BookingDto bookingDto)
    {
        var result = await _bookingsRepository.GetByIdAsync(bookingDto.Id);
        await _bookingsRepository.RemoveAsync(result!);
    }

    public async Task<bool> AnyOverlapAsync(int roomId, DateTimeOffset start, DateTimeOffset end)
    {
        var result = await _bookingsRepository.AnyOverlapAsync(roomId, start, end);
        return result;
    }
}
