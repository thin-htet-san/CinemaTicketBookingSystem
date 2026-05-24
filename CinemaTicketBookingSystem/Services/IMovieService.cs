using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public interface IMovieService
{
    Task<IEnumerable<MovieResponseDto>> GetAllMoviesAsync(bool includeDeleted = false);
    Task<MovieResponseDto?> GetMovieByIdAsync(int id);
    Task<MovieResponseDto> CreateMovieAsync(CreateMovieDto dto);
    Task<MovieResponseDto?> UpdateMovieAsync(int id, UpdateMovieDto dto);
    Task<bool> DeleteMovieAsync(int id);
}
