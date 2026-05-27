using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CinemaTicketBookingSystem.Database.AppDbContextModels;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly AppDbContext _dbContext;

    public ShowtimeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ShowtimeResponseDto>> GetShowtimesAsync(int? movieId = null)
    {
        IQueryable<Showtime> query = _dbContext.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.TheaterHallNavigation)
            .Where(s => !s.IsDeleted);

        if (movieId.HasValue)
        {
            query = query.Where(s => s.MovieId == movieId.Value);
        }

        var showtimes = await query.ToListAsync();
        return showtimes.Select(MapToResponseDto);
    }

    public async Task<ShowtimeResponseDto?> GetShowtimeByIdAsync(int id)
    {
        var showtime = await _dbContext.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.TheaterHallNavigation)
            .FirstOrDefaultAsync(s => s.ShowtimeId == id && !s.IsDeleted);

        return showtime == null ? null : MapToResponseDto(showtime);
    }

    public async Task<ShowtimeResponseDto> CreateShowtimeAsync(CreateShowtimeDto dto)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.MovieId == dto.MovieId && !m.IsDeleted);
        if (movie == null)
        {
            throw new ArgumentException($"Movie with ID {dto.MovieId} does not exist or has been deleted.");
        }

        var hall = await _dbContext.TheaterHalls.FirstOrDefaultAsync(h => h.TheaterHallId == dto.TheaterHallId && !h.IsDeleted);
        if (hall == null)
        {
            throw new ArgumentException($"TheaterHall with ID {dto.TheaterHallId} does not exist or has been deleted.");
        }

        var showtime = new Showtime
        {
            MovieId = dto.MovieId,
            StartTime = dto.StartTime,
            TheaterHallId = dto.TheaterHallId,
            BasePrice = dto.BasePrice,
            IsDeleted = false
        };

        _dbContext.Showtimes.Add(showtime);
        await _dbContext.SaveChangesAsync();

        showtime.Movie = movie;
        showtime.TheaterHallNavigation = hall;

        return MapToResponseDto(showtime);
    }

    public async Task<ShowtimeResponseDto?> UpdateShowtimeAsync(int id, CreateShowtimeDto dto)
    {
        var showtime = await _dbContext.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.TheaterHallNavigation)
            .FirstOrDefaultAsync(s => s.ShowtimeId == id && !s.IsDeleted);

        if (showtime == null)
        {
            return null;
        }

        var movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.MovieId == dto.MovieId && !m.IsDeleted);
        if (movie == null)
        {
            throw new ArgumentException($"Movie with ID {dto.MovieId} does not exist or has been deleted.");
        }

        var hall = await _dbContext.TheaterHalls.FirstOrDefaultAsync(h => h.TheaterHallId == dto.TheaterHallId && !h.IsDeleted);
        if (hall == null)
        {
            throw new ArgumentException($"TheaterHall with ID {dto.TheaterHallId} does not exist or has been deleted.");
        }

        showtime.MovieId = dto.MovieId;
        showtime.StartTime = dto.StartTime;
        showtime.TheaterHallId = dto.TheaterHallId;
        showtime.BasePrice = dto.BasePrice;
        showtime.Movie = movie;
        showtime.TheaterHallNavigation = hall;

        await _dbContext.SaveChangesAsync();

        return MapToResponseDto(showtime);
    }

    public async Task<ShowtimeResponseDto?> UpdateShowtimePatchAsync(int id, PatchShowtimeDto dto)
    {
        var showtime = await _dbContext.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.TheaterHallNavigation)
            .FirstOrDefaultAsync(s => s.ShowtimeId == id && !s.IsDeleted);

        if (showtime == null)
        {
            return null;
        }

        if (dto.MovieId.HasValue)
        {
            var movieExists = await _dbContext.Movies.AnyAsync(m => m.MovieId == dto.MovieId.Value && !m.IsDeleted);
            if (!movieExists)
            {
                throw new ArgumentException($"Movie with ID {dto.MovieId.Value} does not exist or has been deleted.");
            }

            showtime.MovieId = dto.MovieId.Value;
        }

        if (dto.StartTime.HasValue)
        {
            showtime.StartTime = dto.StartTime.Value;
        }

        if (dto.TheaterHallId.HasValue)
        {
            var hall = await _dbContext.TheaterHalls.FirstOrDefaultAsync(h => h.TheaterHallId == dto.TheaterHallId.Value && !h.IsDeleted);
            if (hall == null)
            {
                throw new ArgumentException($"TheaterHall with ID {dto.TheaterHallId.Value} does not exist or has been deleted.");
            }

            showtime.TheaterHallId = dto.TheaterHallId.Value;
            showtime.TheaterHallNavigation = hall;
        }

        if (dto.BasePrice.HasValue)
        {
            if (dto.BasePrice.Value <= 0)
            {
                throw new ArgumentException("BasePrice must be greater than zero.");
            }

            showtime.BasePrice = dto.BasePrice.Value;
        }

        await _dbContext.SaveChangesAsync();

        if (showtime.Movie == null && showtime.MovieId.HasValue)
        {
            showtime.Movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.MovieId == showtime.MovieId.Value);
        }

        if (showtime.TheaterHallNavigation == null && showtime.TheaterHallId.HasValue)
        {
            showtime.TheaterHallNavigation = await _dbContext.TheaterHalls.FirstOrDefaultAsync(h => h.TheaterHallId == showtime.TheaterHallId.Value);
        }

        return MapToResponseDto(showtime);
    }

    public async Task<bool> DeleteShowtimeAsync(int id)
    {
        var showtime = await _dbContext.Showtimes.FirstOrDefaultAsync(s => s.ShowtimeId == id && !s.IsDeleted);
        if (showtime == null)
        {
            return false;
        }

        showtime.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<SeatStatusDto>> GetSeatsForShowtimeAsync(int showtimeId)
    {
        var showtime = await _dbContext.Showtimes
            .Include(s => s.TheaterHallNavigation)
            .FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId && !s.IsDeleted);

        if (showtime == null)
        {
            throw new KeyNotFoundException($"Showtime with ID {showtimeId} was not found.");
        }

        var hall = showtime.TheaterHallNavigation;
        if (hall == null || hall.IsDeleted)
        {
            throw new KeyNotFoundException($"TheaterHall for Showtime ID {showtimeId} was not found.");
        }

        var bookedSeatNumbers = await _dbContext.BookingSeats
            .Where(bs => bs.ShowtimeId == showtimeId)
            .Select(bs => bs.SeatNumber)
            .ToListAsync();

        var bookedSet = new HashSet<string>(bookedSeatNumbers, StringComparer.OrdinalIgnoreCase);
        var seats = new List<SeatStatusDto>();

        for (int r = 0; r < hall.TotalRows; r++)
        {
            var rowLabel = GetRowLabelFromIndex(r);
            var isCoupleRow = r >= hall.CoupleSeatStartRow;
            var maxSeats = isCoupleRow ? hall.SeatsPerRow / 2 : hall.SeatsPerRow;

            for (int seatNumber = 1; seatNumber <= maxSeats; seatNumber++)
            {
                var seatCode = $"{rowLabel}-{seatNumber}";
                seats.Add(new SeatStatusDto
                {
                    SeatNumber = seatCode,
                    Row = rowLabel,
                    Type = isCoupleRow ? "Couple" : "Single",
                    IsAvailable = !bookedSet.Contains(seatCode),
                    Price = isCoupleRow ? showtime.BasePrice * 2 : showtime.BasePrice
                });
            }
        }

        return seats;
    }

    private static ShowtimeResponseDto MapToResponseDto(Showtime showtime)
    {
        return new ShowtimeResponseDto
        {
            ShowtimeId = showtime.ShowtimeId,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie?.Title,
            StartTime = showtime.StartTime,
            TheaterHallId = showtime.TheaterHallId,
            TheaterHallName = showtime.TheaterHallNavigation?.Name,
            BasePrice = showtime.BasePrice,
            IsDeleted = showtime.IsDeleted
        };
    }

    private static string GetRowLabelFromIndex(int rowIndex)
    {
        var labelBuilder = new StringBuilder();
        var index = rowIndex;

        while (index >= 0)
        {
            labelBuilder.Insert(0, (char)('A' + (index % 26)));
            index = (index / 26) - 1;
        }

        return labelBuilder.ToString();
    }
}