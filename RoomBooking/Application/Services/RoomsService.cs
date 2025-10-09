using AutoMapper;
using RoomBooking.Application.Dtos.RoomDtos;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Models;

namespace RoomBooking.Application.Services;

public class RoomsService : IRoomsService
{
    private readonly IRoomsRepository _roomsRepository;
    private readonly IMapper _mapper;

    public RoomsService(IRoomsRepository roomsRepository, IBookingsRepository bookings, IMapper mapper)
    {
        _roomsRepository = roomsRepository;
        _mapper = mapper;
    }

    public async Task<List<RoomDto>> GetAllAsync()
    {
        var result = await _roomsRepository.GetAllAsync();
        return _mapper.Map<List<RoomDto>>(result);
    }

    public async Task<RoomDto?> GetByIdAsync(int id)
    {
        var result = await _roomsRepository.GetByIdAsync(id);
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
}
