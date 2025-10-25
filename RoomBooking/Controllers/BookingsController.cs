using Microsoft.AspNetCore.Mvc;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Application.Services;

namespace RoomBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingsService _bookingsService;
    public BookingsController(IBookingsService bookingsService)
    {
        _bookingsService = bookingsService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest createBookingDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _bookingsService.CreateAsync(createBookingDto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingsService.GetByIdAsync(id);
        if (booking == null) return NotFound();
        return Ok(booking);
    }

    [HttpGet("for-room/{roomId:int}")]
    public async Task<IActionResult> ForRoom(int roomId)
    {
        var bookings = await _bookingsService.GetBookingForRoomAsync(roomId);
        return Ok(bookings);
    }

    [HttpPut("cancel/{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _bookingsService.CancelAsync(id);
        return Ok();
    }

    [HttpPost("swap")]
    public async Task<IActionResult> Swap([FromBody] SwapBookingRequest swapBookingDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var newBooking = await _bookingsService.SwapAsync(swapBookingDto);
        return Ok(new
        {
            NewBooking = newBooking,
            Message = "Booking swapped to new room successfully"
        });
    }

    [HttpGet("user-history/{booker}")]
    public async Task<IActionResult> GetUserHistory(
        string booker,
        [FromQuery] DateTimeOffset? fromDate = null,
        [FromQuery] DateTimeOffset? toDate = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        var bookings = await _bookingsService.GetUserHistoryAsync(booker, fromDate, toDate, minPrice, maxPrice);
        return Ok(bookings);
    }

    [HttpPost("print-user-history/{booker}")]
    public async Task<IActionResult> PrintUserHistory(string booker)
    {
        await _bookingsService.PrintUserHistoryAsync(booker);
        return Ok(new { Message = $"User history for '{booker}' has been logged to the console" });
    }
}
