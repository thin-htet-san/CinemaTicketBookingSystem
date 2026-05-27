using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.Database.AppDbContextModels;
using CinemaTicketBookingSystem.WebApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBookingSystem.WebApi.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _dbContext;

    public ReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Get total revenue by cinema branch
    public async Task<IEnumerable<BranchRevenueDto>> GetRevenueByBranchAsync()
    {
        var bookings = await _dbContext.Bookings
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.TheaterHallNavigation)
                    .ThenInclude(h => h!.CinemaBranch)
            .ToListAsync();

        return bookings
            .Where(IsConfirmed)
            .Where(b => b.Showtime?.TheaterHallNavigation?.CinemaBranch != null)
            .GroupBy(b => new
            {
                BranchId = b.Showtime!.TheaterHallNavigation!.CinemaBranchId,
                BranchName = b.Showtime.TheaterHallNavigation.CinemaBranch.Name
            })
            .Select(g => new BranchRevenueDto
            {
                BranchId = g.Key.BranchId,
                BranchName = g.Key.BranchName,
                TotalRevenue = g.Sum(x => x.TotalAmount)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();
    }

    // Get total revenue by movie
    public async Task<IEnumerable<MovieRevenueDto>> GetRevenueByMovieAsync()
    {
        var bookings = await _dbContext.Bookings
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.BookingSeats)
            .ToListAsync();

        return bookings
            .Where(IsConfirmed)
            .Where(b => b.Showtime?.Movie != null)
            .GroupBy(b => new
            {
                MovieId = b.Showtime!.MovieId ?? 0,
                MovieTitle = b.Showtime.Movie!.Title
            })
            .Select(g => new MovieRevenueDto
            {
                MovieId = g.Key.MovieId,
                MovieTitle = g.Key.MovieTitle,
                TotalTicketsSold = g.Sum(x => x.BookingSeats.Count),
                TotalRevenue = g.Sum(x => x.TotalAmount)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();
    }

    // Get showtime occupancy percentage
    public async Task<ShowtimeOccupancyDto?> GetShowtimeOccupancyAsync(int showtimeId)
    {
        var showtime = await _dbContext.Showtimes
            .Include(s => s.TheaterHallNavigation)
            .FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId && !s.IsDeleted);

        if (showtime == null || showtime.TheaterHallNavigation == null || showtime.TheaterHallNavigation.IsDeleted)
        {
            return null;
        }

        var hall = showtime.TheaterHallNavigation;

        var bookedSeats = await _dbContext.BookingSeats
            .CountAsync(bs => bs.ShowtimeId == showtimeId);

        var singleRows = Math.Min(hall.CoupleSeatStartRow, hall.TotalRows);
        if (singleRows < 0)
        {
            singleRows = 0;
        }

        var coupleRows = hall.TotalRows - singleRows;
        if (coupleRows < 0)
        {
            coupleRows = 0;
        }

        var seatsPerCoupleRow = hall.SeatsPerRow / 2;
        var totalSeats = (singleRows * hall.SeatsPerRow) + (coupleRows * seatsPerCoupleRow);

        var occupancyPercentage = totalSeats == 0
            ? 0
            : Math.Round((decimal)bookedSeats / totalSeats * 100m, 2);

        return new ShowtimeOccupancyDto
        {
            ShowtimeId = showtimeId,
            TotalSeats = totalSeats,
            BookedSeats = bookedSeats,
            OccupancyPercentage = occupancyPercentage
        };
    }

    // Get user spending summary
    public async Task<UserSpendingSummaryDto> GetUserSpendingSummaryAsync(int userId)
    {
        var bookings = await _dbContext.Bookings
            .Include(b => b.BookingSeats)
            .Where(b => b.UserId == userId)
            .ToListAsync();

        var confirmedBookings = bookings.Where(IsConfirmed).ToList();

        return new UserSpendingSummaryDto
        {
            UserId = userId,
            TotalTicketsBought = confirmedBookings.Sum(x => x.BookingSeats.Count),
            TotalAmountSpent = confirmedBookings.Sum(x => x.TotalAmount)
        };
    }

    private static bool IsConfirmed(Booking booking)
    {
        return string.Equals(booking.BookingStatus, "Confirmed", StringComparison.OrdinalIgnoreCase);
    }
}