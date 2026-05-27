using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.Database.AppDbContextModels;
using CinemaTicketBookingSystem.WebApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBookingSystem.WebApi.Services;

public class CinemaBranchService : ICinemaBranchService
{
    private readonly AppDbContext _dbContext;

    public CinemaBranchService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CinemaBranchResponseDto>> GetAllAsync(bool includeDeleted = false)
    {
        IQueryable<CinemaBranch> query = _dbContext.CinemaBranches;
        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        var branches = await query.ToListAsync();
        return branches.Select(MapToResponseDto);
    }

    public async Task<CinemaBranchResponseDto?> GetByIdAsync(int id)
    {
        var branch = await _dbContext.CinemaBranches.FirstOrDefaultAsync(x => x.CinemaBranchId == id && !x.IsDeleted);
        return branch == null ? null : MapToResponseDto(branch);
    }

    public async Task<CinemaBranchResponseDto> CreateAsync(CreateCinemaBranchDto dto)
    {
        var branch = new CinemaBranch
        {
            Name = dto.Name,
            Location = dto.Location,
            IsDeleted = false
        };

        _dbContext.CinemaBranches.Add(branch);
        await _dbContext.SaveChangesAsync();

        return MapToResponseDto(branch);
    }

    public async Task<CinemaBranchResponseDto?> UpdateAsync(int id, UpdateCinemaBranchDto dto)
    {
        var branch = await _dbContext.CinemaBranches.FirstOrDefaultAsync(x => x.CinemaBranchId == id && !x.IsDeleted);
        if (branch == null)
        {
            return null;
        }

        branch.Name = dto.Name;
        branch.Location = dto.Location;

        await _dbContext.SaveChangesAsync();

        return MapToResponseDto(branch);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var branch = await _dbContext.CinemaBranches.FirstOrDefaultAsync(x => x.CinemaBranchId == id && !x.IsDeleted);
        if (branch == null)
        {
            return false;
        }

        branch.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static CinemaBranchResponseDto MapToResponseDto(CinemaBranch branch)
    {
        return new CinemaBranchResponseDto
        {
            CinemaBranchId = branch.CinemaBranchId,
            Name = branch.Name,
            Location = branch.Location,
            IsDeleted = branch.IsDeleted
        };
    }
}