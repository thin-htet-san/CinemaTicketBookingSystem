using System;
using System.Collections.Generic;

namespace CinemaTicketBookingSystem.Database.AppDbContextModels;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? UserId { get; set; }

    public int? ShowtimeId { get; set; }

    public DateTime? BookingTime { get; set; }

    public decimal TotalAmount { get; set; }

    public string BookingStatus { get; set; } = null!;

    public virtual ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();

    public virtual Showtime? Showtime { get; set; }

    public virtual User? User { get; set; }
}
