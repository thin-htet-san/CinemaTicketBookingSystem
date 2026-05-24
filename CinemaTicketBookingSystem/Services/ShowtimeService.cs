using System;
using System.Collections.Generic;
using System.Linq;
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

    // Get all showtimes, with an optional filter by movie ID
    public async Task<IEnumerable<ShowtimeResponseDto>> GetShowtimesAsync(int? movieId = null)
    {
        IQueryable<Showtime> query = _dbContext.Showtimes.Include(s => s.Movie).Where(s => !s.IsDeleted);

        if (movieId.HasValue)
        {
            query = query.Where(s => s.MovieId == movieId.Value);
        }

        var showtimes = await query.ToListAsync();
        return showtimes.Select(MapToResponseDto);
    }


    // Get a showtime by ID
    public async Task<ShowtimeResponseDto?> GetShowtimeByIdAsync(int id)
    {
        var showtime = await _dbContext.Showtimes
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.ShowtimeId == id && !s.IsDeleted);

        if (showtime == null)
        {
            return null;
        }

        return MapToResponseDto(showtime);
    }


    // Create a new showtime
    public async Task<ShowtimeResponseDto> CreateShowtimeAsync(CreateShowtimeDto dto)
    {
        
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.MovieId == dto.MovieId && !m.IsDeleted);
        if (movie == null)
        {
            throw new ArgumentException($"Movie with ID {dto.MovieId} does not exist or has been deleted.");
        }

        var showtime = new Showtime
        {
            MovieId = dto.MovieId,
            StartTime = dto.StartTime,
            TheaterHall = dto.TheaterHall,
            BasePrice = dto.BasePrice,
            IsDeleted = false
        };

        _dbContext.Showtimes.Add(showtime);
        await _dbContext.SaveChangesAsync();

        showtime.Movie = movie;

        return MapToResponseDto(showtime);
    }


    // Update showtime
    public async Task<ShowtimeResponseDto?> UpdateShowtimeAsync(int id, CreateShowtimeDto dto)
    {
        var showtime = await _dbContext.Showtimes
            .Include(s => s.Movie)
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

        showtime.MovieId = dto.MovieId;
        showtime.StartTime = dto.StartTime;
        showtime.TheaterHall = dto.TheaterHall;
        showtime.BasePrice = dto.BasePrice;
        showtime.Movie = movie;

        await _dbContext.SaveChangesAsync();

        return MapToResponseDto(showtime);
    }


    // Delete showtime (soft delete)
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


    // Get seat availability for a showtime
    public async Task<IEnumerable<SeatStatusDto>> GetSeatsForShowtimeAsync(int showtimeId)
    {
        
        var showtimeExists = await _dbContext.Showtimes.AnyAsync(s => s.ShowtimeId == showtimeId && !s.IsDeleted);
        if (!showtimeExists)
        {
            throw new KeyNotFoundException($"Showtime with ID {showtimeId} was not found.");
        }

        var seats = new List<SeatStatusDto>();
        char[] rows = { 'A', 'B', 'C', 'D', 'E', 'F', 'G' };

        for (int r = 0; r < rows.Length; r++)
        {
            char currentRow = rows[r];

            //couple seats so only 5 seats in 1 row 
            int maxSeatsInRow = (currentRow == 'F' || currentRow == 'G') ? 5 : 10;

            for (int seatNum = 1; seatNum <= maxSeatsInRow; seatNum++)
            {
                seats.Add(new SeatStatusDto
                {
                    SeatNumber = $"{currentRow}{seatNum}",
                    IsAvailable = true
                });
            }
        }

        // Get all booked seats for the showtime (from bookign seats table and mark them as unavailable)
        var bookedSeats = await _dbContext.BookingSeats
            .Where(bs => bs.ShowtimeId == showtimeId)
            .Select(bs => bs.SeatNumber)
            .ToListAsync();

        var bookedSet = new HashSet<string>(bookedSeats, StringComparer.OrdinalIgnoreCase);

        foreach (var seat in seats)
        {
            if (bookedSet.Contains(seat.SeatNumber))
            {
                seat.IsAvailable = false;
            }
        }

        return seats;
    }

    // update showtime 
    public async Task<ShowtimeResponseDto> UpdateShowtimePatchAsync(int id, PatchShowtimeDto dto)
    {
        var showtime = await _dbContext.Showtimes.FirstOrDefaultAsync(s => s.ShowtimeId == id && !s.IsDeleted);
        if (showtime == null) return null;

        
        if (dto.MovieId.HasValue)
        {
            showtime.MovieId = dto.MovieId.Value;
        }

        
        if (dto.StartTime.HasValue)
        {
            showtime.StartTime = dto.StartTime.Value;
        }

       
        if (!string.IsNullOrWhiteSpace(dto.TheaterHall))
        {
            showtime.TheaterHall = dto.TheaterHall;
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

        return new ShowtimeResponseDto
        {
            ShowtimeId = showtime.ShowtimeId,
            MovieId = showtime.MovieId,
            StartTime = showtime.StartTime,
            TheaterHall = showtime.TheaterHall,
            BasePrice = showtime.BasePrice,
            IsDeleted = showtime.IsDeleted
        };
    }

    


    private static ShowtimeResponseDto MapToResponseDto(Showtime showtime)
    {
        return new ShowtimeResponseDto
        {
            ShowtimeId = showtime.ShowtimeId,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie?.Title,
            StartTime = showtime.StartTime,
            TheaterHall = showtime.TheaterHall,
            BasePrice = showtime.BasePrice,
            IsDeleted = showtime.IsDeleted
        };
    }
}
