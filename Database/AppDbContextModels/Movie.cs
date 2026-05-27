using System;
using System.Collections.Generic;

namespace CinemaTicketBookingSystem.Database.AppDbContextModels;

public partial class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public int DurationMinutes { get; set; }

    public string Genre { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
