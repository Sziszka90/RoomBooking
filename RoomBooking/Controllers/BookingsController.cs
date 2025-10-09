using Microsoft.AspNetCore.Mvc;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Application.Services;

namespace RoomBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingsService _bookingsService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingsService bookingsService, ILogger<BookingsController> logger)
    {
        _bookingsService = bookingsService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto createBookingDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (createBookingDto.End <= createBookingDto.Start) return BadRequest("End must be after Start");

        try
        {
            var created = await _bookingsService.CreateAsync(createBookingDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
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
        var bookings = await _bookingsService.GetForRoomAsync(roomId);
        return Ok(bookings);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await _bookingsService.GetByIdAsync(id);
        if (booking == null) return NotFound();
        await _bookingsService.CancelAsync(booking);
        return NoContent();
    }
}
