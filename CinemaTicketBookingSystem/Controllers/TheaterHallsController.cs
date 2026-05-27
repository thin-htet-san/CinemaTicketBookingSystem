using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.WebApi.DTOs;
using CinemaTicketBookingSystem.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicketBookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/theater-hall")]
public class TheaterHallsController : ControllerBase
{
    private readonly ITheaterHallService _theaterHallService;

    public TheaterHallsController(ITheaterHallService theaterHallService)
    {
        _theaterHallService = theaterHallService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TheaterHallResponseDto>>> GetAll([FromQuery] bool includeDeleted = false)
    {
        try
        {
            var halls = await _theaterHallService.GetAllAsync(includeDeleted);
            return Ok(halls);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TheaterHallResponseDto>> GetById(int id)
    {
        try
        {
            var hall = await _theaterHallService.GetByIdAsync(id);
            if (hall == null)
            {
                return NotFound(new { message = $"TheaterHall with ID {id} was not found or has been deleted." });
            }

            return Ok(hall);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TheaterHallResponseDto>> Create([FromBody] CreateTheaterHallDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var hall = await _theaterHallService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = hall.TheaterHallId }, hall);
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

    [HttpPut("{id}")]
    public async Task<ActionResult<TheaterHallResponseDto>> Update(int id, [FromBody] UpdateTheaterHallDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var hall = await _theaterHallService.UpdateAsync(id, dto);
            if (hall == null)
            {
                return NotFound(new { message = $"TheaterHall with ID {id} was not found or has been deleted." });
            }

            return Ok(hall);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _theaterHallService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"TheaterHall with ID {id} was not found or has already been deleted." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}