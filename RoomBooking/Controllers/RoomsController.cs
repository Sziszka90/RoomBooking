using Microsoft.AspNetCore.Mvc;
using RoomBooking.Application.Dtos.RoomDtos;
using RoomBooking.Application.Services;

namespace RoomBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomsService _roomsService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IRoomsService rooms, ILogger<RoomsController> logger)
    {
        _roomsService = rooms;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("API request: Get all rooms");
        var rooms = await _roomsService.GetAllAsync();
        _logger.LogInformation("API response: Returning {RoomCount} rooms", rooms.Count);
        return Ok(rooms);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest createRoomDto)
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
    public async Task<IActionResult> GetAvailable(
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice
    )
    {
        if (end <= start) return BadRequest("End must be after start");

        var availableRooms = await _roomsService.GetAvailableRooms(start, end, minPrice, maxPrice);
        return Ok(availableRooms);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("API request: Delete room {RoomId}", id);

        await _roomsService.DeleteAsync(id);
        _logger.LogInformation("API response: Successfully deleted room {RoomId}", id);
        return NoContent();
    }
}
