using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Application.Mapping;
using RoomBooking.Application.Services;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Domain.Exceptions;
using RoomBooking.Domain;
using Xunit;

namespace RoomBooking.Testing.Services;

public class BookingsServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IBookingsRepository> _mockBookingsRepository;
    private readonly Mock<IRoomsRepository> _mockRoomsRepository;
    private readonly Mock<ILogger<BookingsService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly BookingsService _bookingsService;

    public BookingsServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockBookingsRepository = new Mock<IBookingsRepository>();
        _mockRoomsRepository = new Mock<IRoomsRepository>();
        _mockLogger = new Mock<ILogger<BookingsService>>();

        _mockUnitOfWork.Setup(u => u.Bookings).Returns(_mockBookingsRepository.Object);
        _mockUnitOfWork.Setup(u => u.Rooms).Returns(_mockRoomsRepository.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _bookingsService = new BookingsService(
            _mockUnitOfWork.Object,
            _mapper,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateBooking()
    {
        // Arrange
        var today = DateTimeOffset.Now.Date;
        var createBookingDto = new CreateBookingRequest
        {
            RoomId = 1,
            Start = today.AddHours(9),
            End = today.AddDays(1).AddHours(17),
            Booker = "Test User",
        };

        var room = new Room { Id = 1, Name = "Test Room", Capacity = 10, PricePerDay = 50.00m };
        var createdBooking = new Booking
        {
            Id = 1,
            RoomId = 1,
            Start = createBookingDto.Start,
            End = createBookingDto.End,
            Booker = "Test User",
            Room = room,
            TotalPrice = 100.00m,
            BookingDate = DateTimeOffset.Now,
            IsCancelled = false
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
    public async Task CreateAsync_WithInvalidTimeRange_ShouldThrowValidationException()
    {
        // Arrange
        var createBookingDto = new CreateBookingRequest
        {
            RoomId = 1,
            Start = DateTimeOffset.Now.AddHours(3),
            End = DateTimeOffset.Now.AddHours(1),
            Booker = "Test User",
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookingsService.CreateAsync(createBookingDto));

        Assert.Equal("End date must be after start date", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockBookingsRepository.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithLessThanOneDayBooking_ShouldThrowValidationException()
    {
        // Arrange
        var now = DateTimeOffset.Now.Date;
        var createBookingDto = new CreateBookingRequest
        {
            RoomId = 1,
            Start = now.AddHours(9),
            End = now.AddHours(17),
            Booker = "Test User",
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookingsService.CreateAsync(createBookingDto));

        Assert.Equal("End date must be at least one day after start date", exception.Message);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockBookingsRepository.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentRoom_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createBookingDto = new CreateBookingRequest
        {
            RoomId = 999,
            Start = DateTimeOffset.Now,
            End = DateTimeOffset.Now.AddDays(3),
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
    public async Task CreateAsync_WithOverlappingBooking_ShouldThrowOverlapException()
    {
        // Arrange
        var createBookingDto = new CreateBookingRequest
        {
            RoomId = 1,
            Start = DateTimeOffset.Now,
            End = DateTimeOffset.Now.AddDays(3),
            Booker = "Test User"
        };

        var room = new Room { Id = 1, Name = "Test Room", Capacity = 10, PricePerDay = 50.00m };

        _mockRoomsRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);
        _mockBookingsRepository.Setup(b => b.AnyOverlapAsync(1, createBookingDto.Start, createBookingDto.End))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OverlapException>(
            () => _bookingsService.CreateAsync(createBookingDto));

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
    public async Task GetBookingForRoomAsync_ShouldReturnBookingsForRoom()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, RoomId = 1, Start = DateTimeOffset.Now, End = DateTimeOffset.Now.AddHours(1), Booker = "User 1" },
            new Booking { Id = 2, RoomId = 1, Start = DateTimeOffset.Now.AddHours(2), End = DateTimeOffset.Now.AddHours(3), Booker = "User 2" }
        };

        _mockBookingsRepository.Setup(b => b.GetBookingForRoomAsync(1)).ReturnsAsync(bookings);

        // Act
        var result = await _bookingsService.GetBookingForRoomAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("User 1", result[0].Booker);
        Assert.Equal("User 2", result[1].Booker);
        _mockBookingsRepository.Verify(b => b.GetBookingForRoomAsync(1), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_WhenBookingExists_ShouldCancelBooking()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            RoomId = 1,
            Start = DateTimeOffset.Now,
            End = DateTimeOffset.Now.AddHours(2),
            Booker = "Test User",
            IsCancelled = false
        };

        _mockBookingsRepository.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);

        // Act
        await _bookingsService.CancelAsync(1);

        // Assert
        Assert.True(booking.IsCancelled);
        _mockBookingsRepository.Verify(b => b.GetByIdAsync(1), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_WhenBookingDoesNotExist_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        _mockBookingsRepository.Setup(b => b.GetByIdAsync(999)).ReturnsAsync((Booking?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _bookingsService.CancelAsync(999));

        Assert.Equal("Booking 999 not found", exception.Message);
        _mockBookingsRepository.Verify(b => b.GetByIdAsync(999), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task SwapAsync_WithValidData_ShouldSwapBookingToNewRoom()
    {
        // Arrange
        var swapDto = new SwapBookingRequest
        {
            ExistingBookingId = 1,
            NewRoomId = 2
        };

        var existingBooking = new Booking
        {
            Id = 1,
            RoomId = 1,
            Start = DateTimeOffset.Now.AddHours(1),
            End = DateTimeOffset.Now.AddHours(3),
            Booker = "Test User",
            TotalPrice = 100.00m,
            BookingDate = DateTimeOffset.Now.AddDays(-1),
            IsCancelled = false
        };

        var newRoom = new Room { Id = 2, Name = "New Room", Capacity = 4, PricePerDay = 75.00m };

        var newBooking = new Booking
        {
            Id = 2,
            RoomId = 2,
            Start = existingBooking.Start,
            End = existingBooking.End,
            Booker = existingBooking.Booker,
            TotalPrice = 150.00m,
            BookingDate = existingBooking.BookingDate,
            IsCancelled = false
        };

        _mockBookingsRepository.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(existingBooking);
        _mockRoomsRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(newRoom);
        _mockBookingsRepository.Setup(b => b.AnyOverlapAsync(2, existingBooking.Start, existingBooking.End))
            .ReturnsAsync(false);
        _mockBookingsRepository.Setup(b => b.AddAsync(It.IsAny<Booking>())).ReturnsAsync(newBooking);

        // Act
        var result = await _bookingsService.SwapAsync(swapDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal(2, result.RoomId);
        Assert.Equal("Test User", result.Booker);
        Assert.True(existingBooking.IsCancelled);

        _mockBookingsRepository.Verify(b => b.GetByIdAsync(1), Times.Once);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(2), Times.Once);
        _mockBookingsRepository.Verify(b => b.AnyOverlapAsync(2, existingBooking.Start, existingBooking.End), Times.Once);
        _mockBookingsRepository.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SwapAsync_WithNonExistentBooking_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var swapDto = new SwapBookingRequest
        {
            ExistingBookingId = 999,
            NewRoomId = 2
        };

        _mockBookingsRepository.Setup(b => b.GetByIdAsync(999)).ReturnsAsync((Booking?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BookingNotFoundException>(
            () => _bookingsService.SwapAsync(swapDto));

        _mockBookingsRepository.Verify(b => b.GetByIdAsync(999), Times.Once);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SwapAsync_WithNonExistentNewRoom_ShouldThrowRoomNotFoundException()
    {
        // Arrange
        var swapDto = new SwapBookingRequest
        {
            ExistingBookingId = 1,
            NewRoomId = 999
        };

        var existingBooking = new Booking
        {
            Id = 1,
            RoomId = 1,
            Start = DateTimeOffset.Now.AddHours(1),
            End = DateTimeOffset.Now.AddHours(3),
            Booker = "Test User"
        };

        _mockBookingsRepository.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(existingBooking);
        _mockRoomsRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Room?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RoomNotFoundException>(
            () => _bookingsService.SwapAsync(swapDto));

        _mockBookingsRepository.Verify(b => b.GetByIdAsync(1), Times.Once);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task SwapAsync_WithNewRoomNotAvailable_ShouldThrowOverlapException()
    {
        // Arrange
        var swapDto = new SwapBookingRequest
        {
            ExistingBookingId = 1,
            NewRoomId = 2
        };

        var existingBooking = new Booking
        {
            Id = 1,
            RoomId = 1,
            Start = DateTimeOffset.Now.AddHours(1),
            End = DateTimeOffset.Now.AddHours(3),
            Booker = "Test User"
        };

        var newRoom = new Room { Id = 2, Name = "New Room", Capacity = 4, PricePerDay = 75.00m };

        _mockBookingsRepository.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(existingBooking);
        _mockRoomsRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(newRoom);
        _mockBookingsRepository.Setup(b => b.AnyOverlapAsync(2, existingBooking.Start, existingBooking.End))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OverlapException>(
            () => _bookingsService.SwapAsync(swapDto));

        _mockBookingsRepository.Verify(b => b.GetByIdAsync(1), Times.Once);
        _mockRoomsRepository.Verify(r => r.GetByIdAsync(2), Times.Once);
        _mockBookingsRepository.Verify(b => b.AnyOverlapAsync(2, existingBooking.Start, existingBooking.End), Times.Once);
        _mockBookingsRepository.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task GetUserHistoryAsync_WithValidBooker_ShouldReturnUserBookings()
    {
        // Arrange
        var booker = "Test User";
        var fromDate = DateTimeOffset.Now.AddDays(-30);
        var toDate = DateTimeOffset.Now;
        var minPrice = 50m;
        var maxPrice = 200m;

        var bookings = new List<Booking>
        {
            new Booking { Id = 1, RoomId = 1, Booker = booker, TotalPrice = 100m, Start = DateTimeOffset.Now.AddDays(-10) },
            new Booking { Id = 2, RoomId = 2, Booker = booker, TotalPrice = 150m, Start = DateTimeOffset.Now.AddDays(-5) }
        };

        _mockBookingsRepository.Setup(b => b.GetUserHistoryAsync(booker, fromDate, toDate, minPrice, maxPrice))
            .ReturnsAsync(bookings);

        // Act
        var result = await _bookingsService.GetUserHistoryAsync(booker, fromDate, toDate, minPrice, maxPrice);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(booker, result[0].Booker);
        Assert.Equal(booker, result[1].Booker);
        _mockBookingsRepository.Verify(b => b.GetUserHistoryAsync(booker, fromDate, toDate, minPrice, maxPrice), Times.Once);
    }

    [Fact]
    public async Task GetUserHistoryAsync_WithEmptyBooker_ShouldThrowValidationException()
    {
        // Arrange
        var emptyBooker = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookingsService.GetUserHistoryAsync(emptyBooker));

        Assert.Equal("Booker name cannot be empty or null", exception.Message);
        _mockBookingsRepository.Verify(b => b.GetUserHistoryAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset?>(),
            It.IsAny<DateTimeOffset?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>()), Times.Never);
    }

    [Fact]
    public async Task GetUserHistoryAsync_WithNullBooker_ShouldThrowValidationException()
    {
        // Arrange
        string? nullBooker = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookingsService.GetUserHistoryAsync(nullBooker!));

        Assert.Equal("Booker name cannot be empty or null", exception.Message);
        _mockBookingsRepository.Verify(b => b.GetUserHistoryAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset?>(),
            It.IsAny<DateTimeOffset?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>()), Times.Never);
    }

    [Fact]
    public async Task PrintUserHistoryAsync_WithValidBooker_ShouldLogHistory()
    {
        // Arrange
        var booker = "Test User";
        var bookings = new List<Booking>
        {
            new Booking
            {
                Id = 1,
                RoomId = 1,
                Booker = booker,
                TotalPrice = 100m,
                Start = DateTimeOffset.Now.AddDays(-10),
                End = DateTimeOffset.Now.AddDays(-8),
                BookingDate = DateTimeOffset.Now.AddDays(-15),
                IsCancelled = false
            }
        };

        _mockBookingsRepository.Setup(b => b.GetUserHistoryAsync(booker, null, null, null, null))
            .ReturnsAsync(bookings);

        // Act
        await _bookingsService.PrintUserHistoryAsync(booker);

        // Assert
        _mockBookingsRepository.Verify(b => b.GetUserHistoryAsync(booker, null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task PrintUserHistoryAsync_WithEmptyBooker_ShouldThrowValidationException()
    {
        // Arrange
        var emptyBooker = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookingsService.PrintUserHistoryAsync(emptyBooker));

        Assert.Equal("Booker name cannot be empty or null", exception.Message);
        _mockBookingsRepository.Verify(b => b.GetUserHistoryAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset?>(),
            It.IsAny<DateTimeOffset?>(), It.IsAny<decimal?>(), It.IsAny<decimal?>()), Times.Never);
    }

    [Fact]
    public async Task PrintUserHistoryAsync_WithNoBookings_ShouldCompleteSuccessfully()
    {
        // Arrange
        var booker = "User With No Bookings";
        var emptyBookings = new List<Booking>();

        _mockBookingsRepository.Setup(b => b.GetUserHistoryAsync(booker, null, null, null, null))
            .ReturnsAsync(emptyBookings);

        // Act & Assert
        await _bookingsService.PrintUserHistoryAsync(booker);

        _mockBookingsRepository.Verify(b => b.GetUserHistoryAsync(booker, null, null, null, null), Times.Once);
    }
}
