using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.WebApi.DTOs;
using CinemaTicketBookingSystem.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaTicketBookingSystem.WebApi.Controllers;

[ApiController]
[Route("api/cinema-branch")]
public class CinemaBranchesController : ControllerBase
{
    private readonly ICinemaBranchService _cinemaBranchService;

    public CinemaBranchesController(ICinemaBranchService cinemaBranchService)
    {
        _cinemaBranchService = cinemaBranchService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CinemaBranchResponseDto>>> GetAll([FromQuery] bool includeDeleted = false)
    {
        try
        {
            var branches = await _cinemaBranchService.GetAllAsync(includeDeleted);
            return Ok(branches);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CinemaBranchResponseDto>> GetById(int id)
    {
        try
        {
            var branch = await _cinemaBranchService.GetByIdAsync(id);
            if (branch == null)
            {
                return NotFound(new { message = $"CinemaBranch with ID {id} was not found or has been deleted." });
            }

            return Ok(branch);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CinemaBranchResponseDto>> Create([FromBody] CreateCinemaBranchDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var branch = await _cinemaBranchService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = branch.CinemaBranchId }, branch);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CinemaBranchResponseDto>> Update(int id, [FromBody] UpdateCinemaBranchDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var branch = await _cinemaBranchService.UpdateAsync(id, dto);
            if (branch == null)
            {
                return NotFound(new { message = $"CinemaBranch with ID {id} was not found or has been deleted." });
            }

            return Ok(branch);
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
            var deleted = await _cinemaBranchService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"CinemaBranch with ID {id} was not found or has already been deleted." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}