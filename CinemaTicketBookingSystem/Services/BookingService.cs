using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CinemaTicketBookingSystem.Database.AppDbContextModels;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _dbContext;

    public BookingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    //booking create 
    public async Task<BookingReceiptDto> CreateBookingAsync(CreateBookingDto dto)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {dto.UserId} does not exist.");
            }

            var showtime = await _dbContext.Showtimes
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.ShowtimeId == dto.ShowtimeId && !s.IsDeleted);
            if (showtime == null)
            {
                throw new KeyNotFoundException($"Showtime with ID {dto.ShowtimeId} does not exist or has been deleted.");
            }

            var alreadyBookedSeats = await _dbContext.BookingSeats
                .Where(bs => bs.ShowtimeId == dto.ShowtimeId)
                .Select(bs => bs.SeatNumber)
                .ToListAsync();

            var takenSeats = dto.SeatNumbers
                .Intersect(alreadyBookedSeats, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (takenSeats.Any())
            {
                throw new InvalidOperationException($"The following seat(s) are already booked: {string.Join(", ", takenSeats)}.");
            }

            decimal totalAmount = 0;
            foreach (var seat in dto.SeatNumbers)
            {
                if (string.IsNullOrWhiteSpace(seat)) continue;
                char row = char.ToUpper(seat[0]);

                if (row == 'F' || row == 'G') totalAmount += showtime.BasePrice * 2;
                else if (row == 'D' || row == 'E') totalAmount += showtime.BasePrice + 5.00m;
                else totalAmount += showtime.BasePrice;
            }

            var booking = new Booking
            {
                UserId = dto.UserId,
                ShowtimeId = dto.ShowtimeId,
                BookingTime = DateTime.UtcNow,
                TotalAmount = totalAmount,
                BookingStatus = "Confirmed"
            };

            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync(); 

            foreach (var seatNumber in dto.SeatNumbers)
            {
                var bookingSeat = new BookingSeat
                {
                    ShowtimeId = dto.ShowtimeId,
                    SeatNumber = seatNumber,
                    BookingId = booking.BookingId
                };
                _dbContext.BookingSeats.Add(bookingSeat);
            }
            await _dbContext.SaveChangesAsync();

            
            await transaction.CommitAsync();

            
            return new BookingReceiptDto
            {
                BookingId = booking.BookingId,
                UserId = user.UserId,
                UserFullName = user.FullName,
                ShowtimeId = showtime.ShowtimeId,
                MovieTitle = showtime.Movie?.Title ?? "Unknown Movie",
                StartTime = showtime.StartTime,
                TheaterHall = showtime.TheaterHall,
                BookingTime = booking.BookingTime ?? DateTime.UtcNow,
                TotalAmount = booking.TotalAmount,
                BookingStatus = booking.BookingStatus,
                SeatNumbers = dto.SeatNumbers
            };
        }
        catch (Exception)
        {
            
            await transaction.RollbackAsync();
            throw;
        }
    }


    //booking patch status
    public async Task<BookingReceiptDto?> UpdateBookingStatusAsync(int bookingId, string bookingStatus)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.BookingSeats)
            .Include(b => b.User)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null)
        {
            return null;
        }

        var normalizedStatus = bookingStatus.Trim();
        booking.BookingStatus = normalizedStatus;

        if (normalizedStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) &&
            booking.BookingSeats.Any())
        {
            _dbContext.BookingSeats.RemoveRange(booking.BookingSeats);
        }

        await _dbContext.SaveChangesAsync();
        return MapToReceiptDto(booking);
    }


    //booking get all
    public async Task<IEnumerable<BookingReceiptDto>> GetAllBookingsAsync()
    {
        var bookings = await _dbContext.Bookings
            .Include(b => b.User)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.BookingSeats)
            .ToListAsync();

        return bookings.Select(MapToReceiptDto);
    }


    //booking get by id
    public async Task<BookingReceiptDto?> GetBookingByIdAsync(int id)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.User)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.BookingSeats)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return null;
        }

        return MapToReceiptDto(booking);
    }


    //booking get by user id
    public async Task<IEnumerable<BookingReceiptDto>> GetBookingsByUserIdAsync(int userId)
    {
        var bookings = await _dbContext.Bookings
            .Include(b => b.User)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.BookingSeats)
            .Where(b => b.UserId == userId)
            .ToListAsync();

        return bookings.Select(MapToReceiptDto);
    }


    //get seats for booking
    public async Task<IEnumerable<string>> GetSeatsForBookingAsync(int bookingId)
    {
        return await _dbContext.BookingSeats
            .Where(bs => bs.BookingId == bookingId)
            .Select(bs => bs.SeatNumber)
            .ToListAsync();
    }


    private static BookingReceiptDto MapToReceiptDto(Booking booking)
    {
        return new BookingReceiptDto
        {
            BookingId = booking.BookingId,
            UserId = booking.UserId ?? 0,
            UserFullName = booking.User?.FullName ?? "Unknown User",
            ShowtimeId = booking.ShowtimeId ?? 0,
            MovieTitle = booking.Showtime?.Movie?.Title ?? "Unknown Movie",
            StartTime = booking.Showtime?.StartTime ?? DateTime.MinValue,
            TheaterHall = booking.Showtime?.TheaterHall ?? "Unknown Hall",
            BookingTime = booking.BookingTime ?? DateTime.UtcNow,
            TotalAmount = booking.TotalAmount,
            BookingStatus = booking.BookingStatus,
            SeatNumbers = booking.BookingSeats.Select(bs => bs.SeatNumber).ToList()
        };
    }
}
