using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public interface ITheaterHallService
{
    Task<IEnumerable<TheaterHallResponseDto>> GetAllAsync(bool includeDeleted = false);
    Task<TheaterHallResponseDto?> GetByIdAsync(int id);
    Task<TheaterHallResponseDto> CreateAsync(CreateTheaterHallDto dto);
    Task<TheaterHallResponseDto?> UpdateAsync(int id, UpdateTheaterHallDto dto);
    Task<bool> DeleteAsync(int id);
}