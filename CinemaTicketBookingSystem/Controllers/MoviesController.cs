using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CinemaTicketBookingSystem.WebApi.DTOs;
using CinemaTicketBookingSystem.WebApi.Services;

namespace CinemaTicketBookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/movie")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieResponseDto>>> GetAll([FromQuery] bool includeDeleted = false)
    {
        try
        {
            var movies = await _movieService.GetAllMoviesAsync(includeDeleted);
            return Ok(movies);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieResponseDto>> GetById(int id)
    {
        try
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound(new { message = $"Movie with ID {id} was not found or has been deleted." });
            }
            return Ok(movie);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<MovieResponseDto>> Create([FromBody] CreateMovieDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var movie = await _movieService.CreateMovieAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = movie.MovieId }, movie);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MovieResponseDto>> Update(int id, [FromBody] UpdateMovieDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var movie = await _movieService.UpdateMovieAsync(id, dto);
            if (movie == null)
            {
                return NotFound(new { message = $"Movie with ID {id} was not found or has been deleted." });
            }
            return Ok(movie);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<MovieResponseDto>> Patch(int id, [FromBody] PatchMovieDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var movie = await _movieService.PatchMovieAsync(id, dto);
            if (movie == null)
            {
                return NotFound(new { message = $"Movie with ID {id} was not found or has been deleted." });
            }
            return Ok(movie);
        }
        catch (InvalidOperationException ex)
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
            var deleted = await _movieService.DeleteMovieAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Movie with ID {id} was not found or has already been deleted." });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
