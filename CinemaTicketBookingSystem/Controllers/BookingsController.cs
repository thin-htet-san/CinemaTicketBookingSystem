using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CinemaTicketBookingSystem.WebApi.DTOs;
using CinemaTicketBookingSystem.WebApi.Services;

namespace CinemaTicketBookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingReceiptDto>>> GetAll()
    {
        try
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingReceiptDto>> GetById(int id)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking with ID {id} was not found." });
            }
            return Ok(booking);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<BookingReceiptDto>>> GetByUserId(int userId)
    {
        try
        {
            var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/seats")]
    public async Task<ActionResult<IEnumerable<string>>> GetSeats(int id)
    {
        try
        {
            // Verify if booking exists first
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { message = $"Booking with ID {id} was not found." });
            }

            var seats = await _bookingService.GetSeatsForBookingAsync(id);
            return Ok(seats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<BookingReceiptDto>> Create([FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var receipt = await _bookingService.CreateBookingAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = receipt.BookingId }, receipt);
        }
        catch (KeyNotFoundException ex)
        {
            // User or Showtime not found
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Seat already booked
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var cancelled = await _bookingService.CancelBookingAsync(id);
            if (!cancelled)
            {
                return NotFound(new { message = $"Booking with ID {id} was not found." });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
