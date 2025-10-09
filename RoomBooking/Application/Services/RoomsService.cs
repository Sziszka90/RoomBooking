using AutoMapper;
using RoomBooking.Application.Dtos.RoomDtos;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Domain.Exceptions;
using RoomBooking.Models;

namespace RoomBooking.Application.Services;

public class RoomsService : IRoomsService
{
    private readonly IRoomsRepository _roomsRepository;
    private readonly IBookingsRepository _bookingsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RoomsService> _logger;

    public RoomsService(IRoomsRepository roomsRepository, IBookingsRepository bookingsRepository, IMapper mapper, ILogger<RoomsService> logger)
    {
        _roomsRepository = roomsRepository;
        _bookingsRepository = bookingsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<RoomDto>> GetAllAsync()
    {
        var result = await _roomsRepository.GetAllAsync();
        return _mapper.Map<List<RoomDto>>(result);
    }

    public async Task<RoomDto> GetByIdAsync(int id)
    {
        var result = await _roomsRepository.GetByIdAsync(id);
        if (result == null) throw new RoomNotFoundException(id);
        return _mapper.Map<RoomDto>(result);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto createRoomDto)
    {
        var room = new Room { Name = createRoomDto.Name, Capacity = createRoomDto.Capacity };
        var result = await _roomsRepository.AddAsync(room);
        return _mapper.Map<RoomDto>(result);
    }

    public async Task<List<RoomDto>> GetAvailableRooms(DateTimeOffset start, DateTimeOffset end)
    {
        var result = await _roomsRepository.GetAvailableRoomsAsync(start, end);
        return _mapper.Map<List<RoomDto>>(result);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete room {RoomId}", id);

        var room = await _roomsRepository.GetByIdAsync(id);
        if (room == null)
        {
            _logger.LogWarning("Cannot delete room {RoomId}: Room not found", id);
            throw new RoomNotFoundException(id);
        }

        var roomBookings = await _bookingsRepository.GetForRoomAsync(id);
        if (roomBookings.Any())
        {
            _logger.LogWarning("Cannot delete room {RoomId}: Room has {BookingCount} existing bookings",
                id, roomBookings.Count);
            throw new RoomDeletionException("Cannot delete room with existing bookings");
        }

        await _roomsRepository.RemoveAsync(room);
        _logger.LogInformation("Successfully deleted room {RoomId}: {RoomName}", id, room.Name);
        return;
    }
}
