using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public interface IShowtimeService
{
    Task<IEnumerable<ShowtimeResponseDto>> GetShowtimesAsync(int? movieId = null);
    Task<ShowtimeResponseDto?> GetShowtimeByIdAsync(int id);
    Task<ShowtimeResponseDto> CreateShowtimeAsync(CreateShowtimeDto dto);
    Task<ShowtimeResponseDto?> UpdateShowtimeAsync(int id, CreateShowtimeDto dto);

    Task<ShowtimeResponseDto> UpdateShowtimePatchAsync(int id, PatchShowtimeDto dto);
    Task<bool> DeleteShowtimeAsync(int id);
    Task<IEnumerable<SeatStatusDto>> GetSeatsForShowtimeAsync(int showtimeId);
}
