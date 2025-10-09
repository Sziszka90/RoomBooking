using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Application.Mapping;
using RoomBooking.Application.Services;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Domain.Exceptions;
using RoomBooking.Models;
using Xunit;

namespace RoomBooking.Testing.Services;

public class BookingsServiceTests
{
    private readonly Mock<IBookingsRepository> _mockBookingsRepository;
    private readonly Mock<IRoomsRepository> _mockRoomsRepository;
    private readonly Mock<ILogger<BookingsService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly BookingsService _bookingsService;

    public BookingsServiceTests()
    {
        _mockBookingsRepository = new Mock<IBookingsRepository>();
        _mockRoomsRepository = new Mock<IRoomsRepository>();
        _mockLogger = new Mock<ILogger<BookingsService>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _bookingsService = new BookingsService(
            _mockBookingsRepository.Object,
            _mockRoomsRepository.Object,
            _mapper,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateBooking()
    {
        // Arrange
        var createBookingDto = new CreateBookingDto
        {
            RoomId = 1,
            Start = DateTimeOffset.Now.AddHours(1),
            End = DateTimeOffset.Now.AddHours(3),
            Booker = "Test User"
        };

        var room = new Room { Id = 1, Name = "Test Room", Capacity = 10 };
        var createdBooking = new Booking
        {
            Id = 1,
            RoomId = 1,
            Start = createBookingDto.Start,
            End = createBookingDto.End,
            Booker = "Test User",
            Room = room
        };

        _mockRoomsRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);
        _mockBookingsRepository.Setup(b => b.AnyOverlapAsync(1, createBookingDto.Start, createBookingDto.End))
            .ReturnsAsync(false);
        _mockBookingsRepository.Setup(b => b.AddAsync(It.IsAny<Booking>())).ReturnsAsync(createdBooking);

        // Act
        var result = await _bookingsService.CreateAsync(createBookingDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.RoomId);
        Assert.Equal("Test User", result.Booker);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockBookingsRepository.Verify(b => b.AnyOverlapAsync(1, createBookingDto.Start, createBookingDto.End), Times.Once);
        _mockBookingsRepository.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTimeRange_ShouldThrowArgumentException()
    {
        // Arrange
        var createBookingDto = new CreateBookingDto
        {
            RoomId = 1,
            Start = DateTimeOffset.Now.AddHours(3),
            End = DateTimeOffset.Now.AddHours(1),
            Booker = "Test User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _bookingsService.CreateAsync(createBookingDto));

        Assert.Equal("End must be after Start", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockBookingsRepository.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentRoom_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createBookingDto = new CreateBookingDto
        {
            RoomId = 999,
            Start = DateTimeOffset.Now.AddHours(1),
            End = DateTimeOffset.Now.AddHours(3),
            Booker = "Test User"
        };

        _mockRoomsRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Room?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RoomNotFoundException>(
            () => _bookingsService.CreateAsync(createBookingDto));

        Assert.Equal("Room 999 not found", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
        _mockBookingsRepository.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithOverlappingBooking_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createBookingDto = new CreateBookingDto
        {
            RoomId = 1,
            Start = DateTimeOffset.Now.AddHours(1),
            End = DateTimeOffset.Now.AddHours(3),
            Booker = "Test User"
        };

        var room = new Room { Id = 1, Name = "Test Room", Capacity = 10 };

        _mockRoomsRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);
        _mockBookingsRepository.Setup(b => b.AnyOverlapAsync(1, createBookingDto.Start, createBookingDto.End))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bookingsService.CreateAsync(createBookingDto));

        Assert.Equal("Room is already booked for the requested time range", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockBookingsRepository.Verify(b => b.AnyOverlapAsync(1, createBookingDto.Start, createBookingDto.End), Times.Once);
        _mockBookingsRepository.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookingExists_ShouldReturnBooking()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            Start = DateTimeOffset.Now,
            End = DateTimeOffset.Now.AddHours(2),
            Booker = "Test User",
            Room = new Room { Id = 1, Name = "Test Room", Capacity = 10 }
        };

        _mockBookingsRepository.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);

        // Act
        var result = await _bookingsService.GetByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.RoomId);
        Assert.Equal("Test User", result.Booker);
        _mockBookingsRepository.Verify(b => b.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookingDoesNotExist_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        _mockBookingsRepository.Setup(b => b.GetByIdAsync(999)).ReturnsAsync((Booking?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _bookingsService.GetByIdAsync(999));

        Assert.Equal("Booking 999 not found", exception.Message);
        _mockBookingsRepository.Verify(b => b.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task GetForRoomAsync_ShouldReturnBookingsForRoom()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, RoomId = 1, Start = DateTimeOffset.Now, End = DateTimeOffset.Now.AddHours(1), Booker = "User 1" },
            new Booking { Id = 2, RoomId = 1, Start = DateTimeOffset.Now.AddHours(2), End = DateTimeOffset.Now.AddHours(3), Booker = "User 2" }
        };

        _mockBookingsRepository.Setup(b => b.GetForRoomAsync(1)).ReturnsAsync(bookings);

        // Act
        var result = await _bookingsService.GetForRoomAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("User 1", result[0].Booker);
        Assert.Equal("User 2", result[1].Booker);
        _mockBookingsRepository.Verify(b => b.GetForRoomAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookingExists_ShouldDeleteBooking()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            Start = DateTimeOffset.Now,
            End = DateTimeOffset.Now.AddHours(2),
            Booker = "Test User"
        };

        _mockBookingsRepository.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockBookingsRepository.Setup(b => b.RemoveAsync(booking)).Returns(Task.CompletedTask);

        // Act
        await _bookingsService.CancelAsync(1);

        // Assert
        _mockBookingsRepository.Verify(b => b.GetByIdAsync(1), Times.Once);
        _mockBookingsRepository.Verify(b => b.RemoveAsync(booking), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookingDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockBookingsRepository.Setup(b => b.GetByIdAsync(999)).ReturnsAsync((Booking?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _bookingsService.CancelAsync(999));

        Assert.Equal("Booking 999 not found", exception.Message);
        _mockBookingsRepository.Verify(b => b.GetByIdAsync(999), Times.Once);
        _mockBookingsRepository.Verify(b => b.RemoveAsync(It.IsAny<Booking>()), Times.Never);
    }
}
