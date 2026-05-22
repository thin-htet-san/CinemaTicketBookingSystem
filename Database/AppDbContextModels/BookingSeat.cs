using System;
using System.Collections.Generic;

namespace Database.AppDbContextModels;

public partial class BookingSeat
{
    public int ShowtimeId { get; set; }

    public string SeatNumber { get; set; } = null!;

    public int? BookingId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Showtime Showtime { get; set; } = null!;
}
