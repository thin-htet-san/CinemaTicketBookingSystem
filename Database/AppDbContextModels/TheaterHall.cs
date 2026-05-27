using System;
using System.Collections.Generic;

namespace CinemaTicketBookingSystem.Database.AppDbContextModels;

public partial class TheaterHall
{
    public int TheaterHallId { get; set; }

    public int CinemaBranchId { get; set; }

    public string Name { get; set; } = null!;

    public int TotalRows { get; set; }

    public int SeatsPerRow { get; set; }

    public int CoupleSeatStartRow { get; set; }

    public bool IsDeleted { get; set; }

    public virtual CinemaBranch CinemaBranch { get; set; } = null!;

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
