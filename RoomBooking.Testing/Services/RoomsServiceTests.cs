using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RoomBooking.Application.Dtos.RoomDtos;
using RoomBooking.Application.Mapping;
using RoomBooking.Application.Services;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Domain.Exceptions;
using RoomBooking.Domain;
using Xunit;

namespace RoomBooking.Testing.Services;

public class RoomsServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRoomsRepository> _mockRoomsRepository;
    private readonly Mock<IBookingsRepository> _mockBookingsRepository;
    private readonly Mock<ILogger<RoomsService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly RoomsService _roomsService;

    public RoomsServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRoomsRepository = new Mock<IRoomsRepository>();
        _mockBookingsRepository = new Mock<IBookingsRepository>();
        _mockLogger = new Mock<ILogger<RoomsService>>();

        _mockUnitOfWork.Setup(u => u.Rooms).Returns(_mockRoomsRepository.Object);
        _mockUnitOfWork.Setup(u => u.Bookings).Returns(_mockBookingsRepository.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _roomsService = new RoomsService(
            _mockUnitOfWork.Object,
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
        var createRoomDto = new CreateRoomRequest
        {
            Name = "New Room",
            Capacity = 20,
            PricePerDay = 150.00m,
            Description = "A new test room",
            Address = "Budapest"
        };
        var createdRoom = new Room
        {
            Id = 3,
            Name = "New Room",
            Capacity = 20,
            PricePerDay = 150.00m,
            Description = "A new test room",
            Address = "Budapest"
        };

        _mockRoomsRepository.Setup(r => r.AddAsync(It.IsAny<Room>())).ReturnsAsync(createdRoom);

        // Act
        var result = await _roomsService.CreateAsync(createRoomDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("New Room", result.Name);
        Assert.Equal(20, result.Capacity);
        Assert.Equal(150.00m, result.PricePerDay);
        Assert.Equal("A new test room", result.Description);
        Assert.Equal("Budapest", result.Address);
        _mockRoomsRepository.Verify(r => r.AddAsync(It.IsAny<Room>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAvailableRooms_ShouldReturnAvailableRooms()
    {
        // Arrange
        var start = DateTimeOffset.Now;
        var end = start.AddHours(2);
        decimal? minPrice = 50m;
        decimal? maxPrice = 200m;
        var availableRooms = new List<Room>
        {
            new Room { Id = 1, Name = "Available Room", Capacity = 10, PricePerDay = 100m }
        };

        _mockRoomsRepository.Setup(r => r.GetAvailableRoomsAsync(start, end, minPrice, maxPrice))
            .ReturnsAsync(availableRooms);

        // Act
        var result = await _roomsService.GetAvailableRooms(start, end, minPrice, maxPrice);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Available Room", result[0].Name);
        _mockRoomsRepository.Verify(r => r.GetAvailableRoomsAsync(start, end, minPrice, maxPrice), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRoomExists_ShouldDeleteRoom()
    {
        // Arrange
        var roomId = 1;
        var room = new Room { Id = roomId, Name = "Room to Delete", Capacity = 10 };

        _mockRoomsRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockBookingsRepository.Setup(b => b.GetBookingForRoomAsync(roomId)).ReturnsAsync(new List<Booking>());

        // Act
        await _roomsService.DeleteAsync(roomId);

        // Assert
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(roomId), Times.Once);
        _mockBookingsRepository.Verify(b => b.GetBookingForRoomAsync(roomId), Times.Once);
        _mockRoomsRepository.Verify(r => r.Remove(room), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRoomDoesNotExist_ShouldThrowRoomNotFoundException()
    {
        // Arrange
        var roomId = 999;
        _mockRoomsRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync((Room?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RoomNotFoundException>(
            () => _roomsService.DeleteAsync(roomId));

        Assert.Equal("Room 999 not found", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(roomId), Times.Once);
        _mockBookingsRepository.Verify(b => b.GetBookingForRoomAsync(It.IsAny<int>()), Times.Never);
        _mockRoomsRepository.Verify(r => r.Remove(It.IsAny<Room>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenRoomHasBookings_ShouldThrowRoomDeletionException()
    {
        // Arrange
        var roomId = 1;
        var room = new Room { Id = roomId, Name = "Room with Bookings", Capacity = 10 };
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, RoomId = roomId, Start = DateTimeOffset.Now, End = DateTimeOffset.Now.AddHours(1), Booker = "Test User" }
        };

        _mockRoomsRepository.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _mockBookingsRepository.Setup(b => b.GetBookingForRoomAsync(roomId)).ReturnsAsync(bookings);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RoomDeletionException>(
            () => _roomsService.DeleteAsync(roomId));

        Assert.Equal("Cannot delete room with existing bookings", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(roomId), Times.Once);
        _mockBookingsRepository.Verify(b => b.GetBookingForRoomAsync(roomId), Times.Once);
        _mockRoomsRepository.Verify(r => r.Remove(It.IsAny<Room>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailableRooms_WithNullPriceFilters_ShouldReturnAvailableRooms()
    {
        // Arrange
        var start = DateTimeOffset.Now;
        var end = start.AddHours(2);
        var availableRooms = new List<Room>
        {
            new Room { Id = 1, Name = "Available Room", Capacity = 10, PricePerDay = 100m },
            new Room { Id = 2, Name = "Another Room", Capacity = 8, PricePerDay = 150m }
        };

        _mockRoomsRepository.Setup(r => r.GetAvailableRoomsAsync(start, end, null, null))
            .ReturnsAsync(availableRooms);

        // Act
        var result = await _roomsService.GetAvailableRooms(start, end, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Available Room", result[0].Name);
        Assert.Equal("Another Room", result[1].Name);
        _mockRoomsRepository.Verify(r => r.GetAvailableRoomsAsync(start, end, null, null), Times.Once);
    }

    [Fact]
    public async Task GetAvailableRooms_WithMinPriceOnly_ShouldReturnFilteredRooms()
    {
        // Arrange
        var start = DateTimeOffset.Now;
        var end = start.AddHours(2);
        decimal? minPrice = 100m;
        var availableRooms = new List<Room>
        {
            new Room { Id = 1, Name = "Expensive Room", Capacity = 10, PricePerDay = 150m }
        };

        _mockRoomsRepository.Setup(r => r.GetAvailableRoomsAsync(start, end, minPrice, null))
            .ReturnsAsync(availableRooms);

        // Act
        var result = await _roomsService.GetAvailableRooms(start, end, minPrice, null);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Expensive Room", result[0].Name);
        Assert.Equal(150m, result[0].PricePerDay);
        _mockRoomsRepository.Verify(r => r.GetAvailableRoomsAsync(start, end, minPrice, null), Times.Once);
    }

    [Fact]
    public async Task GetAvailableRooms_WithMaxPriceOnly_ShouldReturnFilteredRooms()
    {
        // Arrange
        var start = DateTimeOffset.Now;
        var end = start.AddHours(2);
        decimal? maxPrice = 100m;
        var availableRooms = new List<Room>
        {
            new Room { Id = 1, Name = "Budget Room", Capacity = 6, PricePerDay = 75m }
        };

        _mockRoomsRepository.Setup(r => r.GetAvailableRoomsAsync(start, end, null, maxPrice))
            .ReturnsAsync(availableRooms);

        // Act
        var result = await _roomsService.GetAvailableRooms(start, end, null, maxPrice);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Budget Room", result[0].Name);
        Assert.Equal(75m, result[0].PricePerDay);
        _mockRoomsRepository.Verify(r => r.GetAvailableRoomsAsync(start, end, null, maxPrice), Times.Once);
    }

    [Fact]
    public async Task GetAvailableRooms_WithNoAvailableRooms_ShouldReturnEmptyList()
    {
        // Arrange
        var start = DateTimeOffset.Now;
        var end = start.AddHours(2);
        var emptyRooms = new List<Room>();

        _mockRoomsRepository.Setup(r => r.GetAvailableRoomsAsync(start, end, null, null))
            .ReturnsAsync(emptyRooms);

        // Act
        var result = await _roomsService.GetAvailableRooms(start, end, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRoomsRepository.Verify(r => r.GetAvailableRoomsAsync(start, end, null, null), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleRooms_ShouldReturnAllRoomsWithCompleteData()
    {
        // Arrange
        var rooms = new List<Room>
        {
            new Room { Id = 1, Name = "Room A", Capacity = 10, PricePerDay = 100m, Description = "Nice room", Address = "Budapest" },
            new Room { Id = 2, Name = "Room B", Capacity = 15, PricePerDay = 150m, Description = "Luxury room", Address = "Budapest" }
        };
        _mockRoomsRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);

        // Act
        var result = await _roomsService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Room A", result[0].Name);
        Assert.Equal(100m, result[0].PricePerDay);
        Assert.Equal("Nice room", result[0].Description);
        Assert.Equal("Room B", result[1].Name);
        Assert.Equal(150m, result[1].PricePerDay);
        Assert.Equal("Luxury room", result[1].Description);
        _mockRoomsRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithCompleteRoomData_ShouldReturnAllProperties()
    {
        // Arrange
        var room = new Room
        {
            Id = 1,
            Name = "Complete Room",
            Capacity = 10,
            PricePerDay = 125m,
            Description = "A complete room with all details",
            Address = "Budapest"
        };
        _mockRoomsRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);

        // Act
        var result = await _roomsService.GetByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Complete Room", result.Name);
        Assert.Equal(10, result.Capacity);
        Assert.Equal(125m, result.PricePerDay);
        Assert.Equal("A complete room with all details", result.Description);
        Assert.Equal("Budapest", result.Address);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }
}
