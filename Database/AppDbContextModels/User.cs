using System;
using System.Collections.Generic;

namespace Database.AppDbContextModels;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
