using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.Database.AppDbContextModels;
using CinemaTicketBookingSystem.WebApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBookingSystem.WebApi.Services;

public class TheaterHallService : ITheaterHallService
{
    private readonly AppDbContext _dbContext;

    public TheaterHallService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    // Get all theater halls 
    public async Task<IEnumerable<TheaterHallResponseDto>> GetAllAsync(bool includeDeleted = false)
    {
        IQueryable<TheaterHall> query = _dbContext.TheaterHalls.Include(x => x.CinemaBranch);
        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        var halls = await query.ToListAsync();
        return halls.Select(MapToResponseDto);
    }

    //Get Theater Hall by Id
    public async Task<TheaterHallResponseDto?> GetByIdAsync(int id)
    {
        var hall = await _dbContext.TheaterHalls
            .Include(x => x.CinemaBranch)
            .FirstOrDefaultAsync(x => x.TheaterHallId == id && !x.IsDeleted);

        return hall == null ? null : MapToResponseDto(hall);
    }

    // Create a new theater hall
    public async Task<TheaterHallResponseDto> CreateAsync(CreateTheaterHallDto dto)
    {
        await ValidateCinemaBranchAsync(dto.CinemaBranchId);
        ValidateHallNumbers(dto.TotalRows, dto.SeatsPerRow, dto.CoupleSeatStartRow);

        var hall = new TheaterHall
        {
            CinemaBranchId = dto.CinemaBranchId,
            Name = dto.Name,
            TotalRows = dto.TotalRows,
            SeatsPerRow = dto.SeatsPerRow,
            CoupleSeatStartRow = dto.CoupleSeatStartRow,
            IsDeleted = false
        };

        _dbContext.TheaterHalls.Add(hall);
        await _dbContext.SaveChangesAsync();

        hall.CinemaBranch = await _dbContext.CinemaBranches.FirstAsync(x => x.CinemaBranchId == dto.CinemaBranchId);

        return MapToResponseDto(hall);
    }

    // Update theater hall
    public async Task<TheaterHallResponseDto?> UpdateAsync(int id, UpdateTheaterHallDto dto)
    {
        var hall = await _dbContext.TheaterHalls
            .Include(x => x.CinemaBranch)
            .FirstOrDefaultAsync(x => x.TheaterHallId == id && !x.IsDeleted);

        if (hall == null)
        {
            return null;
        }

        await ValidateCinemaBranchAsync(dto.CinemaBranchId);
        ValidateHallNumbers(dto.TotalRows, dto.SeatsPerRow, dto.CoupleSeatStartRow);

        hall.CinemaBranchId = dto.CinemaBranchId;
        hall.Name = dto.Name;
        hall.TotalRows = dto.TotalRows;
        hall.SeatsPerRow = dto.SeatsPerRow;
        hall.CoupleSeatStartRow = dto.CoupleSeatStartRow;

        await _dbContext.SaveChangesAsync();

        hall.CinemaBranch = await _dbContext.CinemaBranches.FirstAsync(x => x.CinemaBranchId == dto.CinemaBranchId);

        return MapToResponseDto(hall);
    }

    // Soft delete theater hall
    public async Task<bool> DeleteAsync(int id)
    {
        var hall = await _dbContext.TheaterHalls.FirstOrDefaultAsync(x => x.TheaterHallId == id && !x.IsDeleted);
        if (hall == null)
        {
            return false;
        }

        hall.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    
    private async Task ValidateCinemaBranchAsync(int cinemaBranchId)
    {
        var exists = await _dbContext.CinemaBranches.AnyAsync(x => x.CinemaBranchId == cinemaBranchId && !x.IsDeleted);
        if (!exists)
        {
            throw new ArgumentException($"CinemaBranch with ID {cinemaBranchId} does not exist or has been deleted.");
        }
    }

    private static void ValidateHallNumbers(int totalRows, int seatsPerRow, int coupleSeatStartRow)
    {
        if (totalRows <= 0)
        {
            throw new ArgumentException("TotalRows must be greater than zero.");
        }

        if (seatsPerRow <= 0)
        {
            throw new ArgumentException("SeatsPerRow must be greater than zero.");
        }

        if (coupleSeatStartRow < 0 || coupleSeatStartRow > totalRows)
        {
            throw new ArgumentException("CoupleSeatStartRow must be between 0 and TotalRows.");
        }
    }

    private static TheaterHallResponseDto MapToResponseDto(TheaterHall hall)
    {
        return new TheaterHallResponseDto
        {
            TheaterHallId = hall.TheaterHallId,
            CinemaBranchId = hall.CinemaBranchId,
            CinemaBranchName = hall.CinemaBranch?.Name ?? string.Empty,
            Name = hall.Name,
            TotalRows = hall.TotalRows,
            SeatsPerRow = hall.SeatsPerRow,
            CoupleSeatStartRow = hall.CoupleSeatStartRow,
            IsDeleted = hall.IsDeleted
        };
    }
}