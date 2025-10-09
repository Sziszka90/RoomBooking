using Microsoft.AspNetCore.Mvc;
using RoomBooking.Application.Dtos.RoomDtos;
using RoomBooking.Application.Services;

namespace RoomBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomsService _roomsService;
    private readonly IBookingsService _bookingsService;

    public RoomsController(IRoomsService rooms, IBookingsService bookings)
    {
        _roomsService = rooms;
        _bookingsService = bookings;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _roomsService.GetAllAsync();
        return Ok(rooms);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto createRoomDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var room = await _roomsService.CreateAsync(createRoomDto);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _roomsService.GetByIdAsync(id);
        if (room == null) return NotFound();
        return Ok(room);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        if (end <= start) return BadRequest("End must be after start");

        var availableRooms = await _roomsService.GetAvailableRooms(start, end);
        return Ok(availableRooms);
    }
}
