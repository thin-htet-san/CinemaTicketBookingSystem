using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.WebApi.DTOs;
using CinemaTicketBookingSystem.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicketBookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("revenue/branches")]
    public async Task<ActionResult<IEnumerable<BranchRevenueDto>>> GetRevenueByBranches()
    {
        try
        {
            var result = await _reportService.GetRevenueByBranchAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("revenue/movies")]
    public async Task<ActionResult<IEnumerable<MovieRevenueDto>>> GetRevenueByMovies()
    {
        try
        {
            var result = await _reportService.GetRevenueByMovieAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("occupancy/{showtimeId}")]
    public async Task<ActionResult<ShowtimeOccupancyDto>> GetShowtimeOccupancy(int showtimeId)
    {
        try
        {
            var result = await _reportService.GetShowtimeOccupancyAsync(showtimeId);
            if (result == null)
            {
                return NotFound(new { message = $"Showtime with ID {showtimeId} was not found." });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("user/{userId}/summary")]
    public async Task<ActionResult<UserSpendingSummaryDto>> GetUserSummary(int userId)
    {
        try
        {
            var result = await _reportService.GetUserSpendingSummaryAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}