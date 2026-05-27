using System;
using System.Collections.Generic;

namespace CinemaTicketBookingSystem.Database.AppDbContextModels;

public partial class CinemaBranch
{
    public int CinemaBranchId { get; set; }

    public string Name { get; set; } = null!;

    public string? Location { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<TheaterHall> TheaterHalls { get; set; } = new List<TheaterHall>();
}
