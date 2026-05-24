using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public interface IBookingService
{
    Task<BookingReceiptDto> CreateBookingAsync(CreateBookingDto dto);
    Task<bool> CancelBookingAsync(int bookingId);
    Task<IEnumerable<BookingReceiptDto>> GetAllBookingsAsync();
    Task<BookingReceiptDto?> GetBookingByIdAsync(int id);
    Task<IEnumerable<BookingReceiptDto>> GetBookingsByUserIdAsync(int userId);
    Task<IEnumerable<string>> GetSeatsForBookingAsync(int bookingId);
}
