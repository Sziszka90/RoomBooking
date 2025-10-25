using AutoMapper;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Domain.Exceptions;
using RoomBooking.Domain;
using RoomBooking.Application.Dtos.RoomDtos;

namespace RoomBooking.Application.Services;

public class RoomsService : IRoomsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RoomsService> _logger;

    public RoomsService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RoomsService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<RoomResponse>> GetAllAsync()
    {
        var result = await _unitOfWork.Rooms.GetAllAsync();
        return _mapper.Map<List<RoomResponse>>(result);
    }

    public async Task<RoomResponse> GetByIdAsync(int id)
    {
        var result = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (result == null) throw new RoomNotFoundException(id);
        return _mapper.Map<RoomResponse>(result);
    }

    public async Task<RoomResponse> CreateAsync(CreateRoomRequest createRoomDto)
    {
        var room = _mapper.Map<Room>(createRoomDto);
        var result = await _unitOfWork.Rooms.AddAsync(room);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<RoomResponse>(result);
    }

    public async Task<List<RoomResponse>> GetAvailableRooms(
        DateTimeOffset start,
        DateTimeOffset end,
        decimal? minPrice,
        decimal? maxPrice)
    {
        var result = await _unitOfWork.Rooms.GetAvailableRoomsAsync(start, end, minPrice, maxPrice);
        return _mapper.Map<List<RoomResponse>>(result);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete room {RoomId}", id);

        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room == null)
        {
            _logger.LogWarning("Cannot delete room {RoomId}: Room not found", id);
            throw new RoomNotFoundException(id);
        }

        var roomBookings = await _unitOfWork.Bookings.GetBookingForRoomAsync(id);
        if (roomBookings.Any())
        {
            _logger.LogWarning("Cannot delete room {RoomId}: Room has {BookingCount} existing bookings",
                id, roomBookings.Count);
            throw new RoomDeletionException("Cannot delete room with existing bookings");
        }

        _unitOfWork.Rooms.Remove(room);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Successfully deleted room {RoomId}: {RoomName}", id, room.Name);
        return;
    }
}
