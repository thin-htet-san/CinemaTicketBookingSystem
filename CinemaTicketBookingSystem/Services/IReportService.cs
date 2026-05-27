using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public interface IReportService
{
    Task<IEnumerable<BranchRevenueDto>> GetRevenueByBranchAsync();
    Task<IEnumerable<MovieRevenueDto>> GetRevenueByMovieAsync();
    Task<ShowtimeOccupancyDto?> GetShowtimeOccupancyAsync(int showtimeId);
    Task<UserSpendingSummaryDto> GetUserSpendingSummaryAsync(int userId);
}