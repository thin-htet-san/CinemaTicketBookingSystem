using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CinemaTicketBookingSystem.WebApi.DTOs;
using CinemaTicketBookingSystem.WebApi.Services;

namespace CinemaTicketBookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/showtime")]
public class ShowtimesController : ControllerBase
{
    private readonly IShowtimeService _showtimeService;

    public ShowtimesController(IShowtimeService showtimeService)
    {
        _showtimeService = showtimeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShowtimeResponseDto>>> Get([FromQuery] int? movieId)
    {
        try
        {
            var showtimes = await _showtimeService.GetShowtimesAsync(movieId);
            return Ok(showtimes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShowtimeResponseDto>> GetById(int id)
    {
        try
        {
            var showtime = await _showtimeService.GetShowtimeByIdAsync(id);
            if (showtime == null)
            {
                return NotFound(new { message = $"Showtime with ID {id} was not found or has been deleted." });
            }
            return Ok(showtime);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/seat")]
    public async Task<ActionResult<IEnumerable<SeatStatusDto>>> GetSeats(int id)
    {
        try
        {
            var seats = await _showtimeService.GetSeatsForShowtimeAsync(id);
            return Ok(seats);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ShowtimeResponseDto>> Create([FromBody] CreateShowtimeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var showtime = await _showtimeService.CreateShowtimeAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = showtime.ShowtimeId }, showtime);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ShowtimeResponseDto>> Patch(int id, [FromBody] PatchShowtimeDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Patch data cannot be null." });
        }

        try
        {
            var showtime = await _showtimeService.UpdateShowtimePatchAsync(id, dto);
            if (showtime == null)
            {
                return NotFound(new { message = $"Showtime with ID {id} was not found or has been deleted." });
            }
            return Ok(showtime);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _showtimeService.DeleteShowtimeAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Showtime with ID {id} was not found or has already been deleted." });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
