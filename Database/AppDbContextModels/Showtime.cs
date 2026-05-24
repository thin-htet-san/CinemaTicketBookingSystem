using System;
using System.Collections.Generic;

namespace CinemaTicketBookingSystem.Database.AppDbContextModels;

public partial class Showtime
{
    public int ShowtimeId { get; set; }

    public int? MovieId { get; set; }

    public DateTime StartTime { get; set; }

    public string TheaterHall { get; set; } = null!;

    public decimal BasePrice { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Movie? Movie { get; set; }
}
