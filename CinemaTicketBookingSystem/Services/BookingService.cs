using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

    // Create a new booking
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
                .Include(s => s.TheaterHallNavigation)
                .FirstOrDefaultAsync(s => s.ShowtimeId == dto.ShowtimeId && !s.IsDeleted);
            if (showtime == null)
            {
                throw new KeyNotFoundException($"Showtime with ID {dto.ShowtimeId} does not exist or has been deleted.");
            }
            if (showtime.StartTime < DateTime.Now)
            {
                throw new InvalidOperationException($"Cannot book tickets. This showtime has already passed (Scheduled: {showtime.StartTime}).");
            }

            var hall = showtime.TheaterHallNavigation;
            if (hall == null || hall.IsDeleted)
            {
                throw new KeyNotFoundException($"TheaterHall for Showtime ID {dto.ShowtimeId} does not exist or has been deleted.");
            }

            var normalizedSeatNumbers = new List<string>();
            var duplicateSeatSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            decimal totalAmount = 0;

            foreach (var rawSeatNumber in dto.SeatNumbers)
            {
                var normalizedSeat = NormalizeSeatNumber(rawSeatNumber);
                if (!duplicateSeatSet.Add(normalizedSeat))
                {
                    throw new InvalidOperationException($"Duplicate seat in request: {normalizedSeat}.");
                }

                if (!TryParseSeatNumber(normalizedSeat, out var rowLabel, out var seatIndex))
                {
                    throw new InvalidOperationException($"Invalid seat format: {rawSeatNumber}. Use format like A-1.");
                }

                var rowIndex = GetRowIndexFromLabel(rowLabel);
                if (rowIndex < 0 || rowIndex >= hall.TotalRows)
                {
                    throw new InvalidOperationException($"Seat {normalizedSeat} is outside the hall row range.");
                }

                var isCoupleRow = rowIndex >= hall.CoupleSeatStartRow;
                var maxSeatsInRow = isCoupleRow ? hall.SeatsPerRow / 2 : hall.SeatsPerRow;
                if (seatIndex < 1 || seatIndex > maxSeatsInRow)
                {
                    throw new InvalidOperationException($"Seat {normalizedSeat} is outside the seat range for row {rowLabel}.");
                }

                totalAmount += isCoupleRow ? showtime.BasePrice * 2 : showtime.BasePrice;
                normalizedSeatNumbers.Add(normalizedSeat);
            }

            var alreadyBookedSeats = await _dbContext.BookingSeats
                .Where(bs => bs.ShowtimeId == dto.ShowtimeId)
                .Select(bs => bs.SeatNumber)
                .ToListAsync();

            var takenSeats = normalizedSeatNumbers
                .Intersect(alreadyBookedSeats, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (takenSeats.Any())
            {
                throw new InvalidOperationException($"The following seat(s) are already booked: {string.Join(", ", takenSeats)}.");
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

            foreach (var seatNumber in normalizedSeatNumbers)
            {
                _dbContext.BookingSeats.Add(new BookingSeat
                {
                    ShowtimeId = dto.ShowtimeId,
                    SeatNumber = seatNumber,
                    BookingId = booking.BookingId
                });
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
                TheaterHall = showtime.TheaterHallNavigation?.Name ?? "Unknown Hall",
                BookingTime = booking.BookingTime ?? DateTime.UtcNow,
                TotalAmount = booking.TotalAmount,
                BookingStatus = booking.BookingStatus,
                SeatNumbers = normalizedSeatNumbers
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Update booking status (e.g., Cancelled)
    public async Task<BookingReceiptDto?> UpdateBookingStatusAsync(int bookingId, string bookingStatus)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.BookingSeats)
            .Include(b => b.User)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.TheaterHallNavigation)
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

    // Retrieve bookings
    public async Task<IEnumerable<BookingReceiptDto>> GetAllBookingsAsync()
    {
        var bookings = await _dbContext.Bookings
            .Include(b => b.User)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.TheaterHallNavigation)
            .Include(b => b.BookingSeats)
            .ToListAsync();

        return bookings.Select(MapToReceiptDto);
    }

    public async Task<BookingReceiptDto?> GetBookingByIdAsync(int id)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.User)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.TheaterHallNavigation)
            .Include(b => b.BookingSeats)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return null;
        }

        return MapToReceiptDto(booking);
    }

    // Retrieve bookings by user ID
    public async Task<IEnumerable<BookingReceiptDto>> GetBookingsByUserIdAsync(int userId)
    {
        var bookings = await _dbContext.Bookings
            .Include(b => b.User)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.TheaterHallNavigation)
            .Include(b => b.BookingSeats)
            .Where(b => b.UserId == userId)
            .ToListAsync();

        return bookings.Select(MapToReceiptDto);
    }


    // Retrieve booked seats for a specific booking
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
            TheaterHall = booking.Showtime?.TheaterHallNavigation?.Name ?? "Unknown Hall",
            BookingTime = booking.BookingTime ?? DateTime.UtcNow,
            TotalAmount = booking.TotalAmount,
            BookingStatus = booking.BookingStatus,
            SeatNumbers = booking.BookingSeats.Select(bs => bs.SeatNumber).ToList()
        };
    }

    private static string NormalizeSeatNumber(string seatNumber)
    {
        if (string.IsNullOrWhiteSpace(seatNumber))
        {
            throw new InvalidOperationException("Seat number cannot be empty.");
        }

        var trimmed = seatNumber.Trim().ToUpperInvariant();
        if (!TryParseSeatNumber(trimmed, out var rowLabel, out var seatIndex))
        {
            throw new InvalidOperationException($"Invalid seat format: {seatNumber}. Use format like A-1.");
        }

        return $"{rowLabel}-{seatIndex}";
    }

    private static bool TryParseSeatNumber(string seatNumber, out string rowLabel, out int seatIndex)
    {
        rowLabel = string.Empty;
        seatIndex = 0;

        var parts = seatNumber.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        var parsedRow = parts[0].Trim().ToUpperInvariant();
        if (parsedRow.Length == 0 || !parsedRow.All(char.IsLetter))
        {
            return false;
        }

        if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var parsedSeat) || parsedSeat <= 0)
        {
            return false;
        }

        rowLabel = parsedRow;
        seatIndex = parsedSeat;
        return true;
    }

    private static int GetRowIndexFromLabel(string rowLabel)
    {
        var label = rowLabel.ToUpperInvariant();
        var index = 0;

        foreach (var ch in label)
        {
            index = (index * 26) + (ch - 'A' + 1);
        }

        return index - 1;
    }
}