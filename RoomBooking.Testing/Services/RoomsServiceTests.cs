using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RoomBooking.Application.Dtos.RoomDtos;
using RoomBooking.Application.Mapping;
using RoomBooking.Application.Services;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Domain.Exceptions;
using RoomBooking.Models;
using Xunit;

namespace RoomBooking.Testing.Services;

public class RoomsServiceTests
{
    private readonly Mock<IRoomsRepository> _mockRoomsRepository;
    private readonly Mock<IBookingsRepository> _mockBookingsRepository;
    private readonly Mock<ILogger<RoomsService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly RoomsService _roomsService;

    public RoomsServiceTests()
    {
        _mockRoomsRepository = new Mock<IRoomsRepository>();
        _mockBookingsRepository = new Mock<IBookingsRepository>();
        _mockLogger = new Mock<ILogger<RoomsService>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _roomsService = new RoomsService(
            _mockRoomsRepository.Object,
            _mockBookingsRepository.Object,
            _mapper,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRooms()
    {
        // Arrange
        var rooms = new List<Room>
        {
            new Room { Id = 1, Name = "Room A", Capacity = 10 },
            new Room { Id = 2, Name = "Room B", Capacity = 15 }
        };
        _mockRoomsRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);

        // Act
        var result = await _roomsService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Room A", result[0].Name);
        Assert.Equal("Room B", result[1].Name);
        _mockRoomsRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRoomExists_ShouldReturnRoom()
    {
        // Arrange
        var room = new Room { Id = 1, Name = "Room A", Capacity = 10 };
        _mockRoomsRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);

        // Act
        var result = await _roomsService.GetByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Room A", result.Name);
        Assert.Equal(10, result.Capacity);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRoomDoesNotExist_ShouldThrowRoomNotFoundException()
    {
        // Arrange
        _mockRoomsRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Room?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RoomNotFoundException>(
            () => _roomsService.GetByIdAsync(999));

        Assert.Equal("Room 999 not found", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnRoom()
    {
        // Arrange
        var createRoomDto = new CreateRoomDto { Name = "New Room", Capacity = 20 };
        var createdRoom = new Room { Id = 3, Name = "New Room", Capacity = 20 };

        _mockRoomsRepository.Setup(r => r.AddAsync(It.IsAny<Room>())).ReturnsAsync(createdRoom);

        // Act
        var result = await _roomsService.CreateAsync(createRoomDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("New Room", result.Name);
        Assert.Equal(20, result.Capacity);
        _mockRoomsRepository.Verify(r => r.AddAsync(It.Is<Room>(room =>
            room.Name == "New Room" && room.Capacity == 20)), Times.Once);
    }

    [Fact]
    public async Task GetAvailableRooms_ShouldReturnAvailableRooms()
    {
        // Arrange
        var start = DateTimeOffset.Now;
        var end = start.AddHours(2);
        var availableRooms = new List<Room>
        {
            new Room { Id = 1, Name = "Available Room", Capacity = 10 }
        };

        _mockRoomsRepository.Setup(r => r.GetAvailableRoomsAsync(start, end))
            .ReturnsAsync(availableRooms);

        // Act
        var result = await _roomsService.GetAvailableRooms(start, end);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Available Room", result[0].Name);
        _mockRoomsRepository.Verify(r => r.GetAvailableRoomsAsync(start, end), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRoomExists_ShouldDeleteRoom()
    {
        // Arrange
        var roomId = 1;
        var room = new Room { Id = roomId, Name = "Room to Delete", Capacity = 10 };

        _mockRoomsRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockBookingsRepository.Setup(b => b.GetForRoomAsync(roomId)).ReturnsAsync(new List<Booking>());
        _mockRoomsRepository.Setup(r => r.RemoveAsync(room)).Returns(Task.CompletedTask);

        // Act
        await _roomsService.DeleteAsync(roomId);

        // Assert
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(roomId), Times.Once);
        _mockBookingsRepository.Verify(b => b.GetForRoomAsync(roomId), Times.Once);
        _mockRoomsRepository.Verify(r => r.RemoveAsync(room), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRoomDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var roomId = 999;
        _mockRoomsRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync((Room?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RoomNotFoundException>(
            () => _roomsService.DeleteAsync(roomId));

        Assert.Equal("Room 999 not found", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(roomId), Times.Once);
        _mockBookingsRepository.Verify(b => b.GetForRoomAsync(It.IsAny<int>()), Times.Never);
        _mockRoomsRepository.Verify(r => r.RemoveAsync(It.IsAny<Room>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenRoomHasBookings_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var roomId = 1;
        var room = new Room { Id = roomId, Name = "Room with Bookings", Capacity = 10 };
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, RoomId = roomId, Start = DateTimeOffset.Now, End = DateTimeOffset.Now.AddHours(1), Booker = "Test User" }
        };

        _mockRoomsRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockBookingsRepository.Setup(b => b.GetForRoomAsync(roomId)).ReturnsAsync(bookings);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RoomDeletionException>(
            () => _roomsService.DeleteAsync(roomId));

        Assert.Equal("Cannot delete room with existing bookings", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(roomId), Times.Once);
        _mockBookingsRepository.Verify(b => b.GetForRoomAsync(roomId), Times.Once);
        _mockRoomsRepository.Verify(r => r.RemoveAsync(It.IsAny<Room>()), Times.Never);
    }
}
