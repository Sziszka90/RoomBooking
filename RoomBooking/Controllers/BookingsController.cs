using Microsoft.AspNetCore.Mvc;
using RoomBooking.Application.Dtos.BookingDtos;
using RoomBooking.Application.Services;
using RoomBooking.Domain.Exceptions;

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
    public async Task<IActionResult> Create([FromBody] CreateBookingDto createBookingDto)
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
    public async Task<IActionResult> Swap([FromBody] SwapBookingDto swapBookingDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var newBooking = await _bookingsService.SwapAsync(swapBookingDto);
        return Ok(new
        {
            NewBooking = newBooking,
            Message = "Booking swapped to new room successfully"
        });
    }
}
