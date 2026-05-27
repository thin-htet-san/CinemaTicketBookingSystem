using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public interface ICinemaBranchService
{
    Task<IEnumerable<CinemaBranchResponseDto>> GetAllAsync(bool includeDeleted = false);
    Task<CinemaBranchResponseDto?> GetByIdAsync(int id);
    Task<CinemaBranchResponseDto> CreateAsync(CreateCinemaBranchDto dto);
    Task<CinemaBranchResponseDto?> UpdateAsync(int id, UpdateCinemaBranchDto dto);
    Task<bool> DeleteAsync(int id);
}