namespace CinemaTicketBookingSystem.WebApi.DTOs;

public class BranchRevenueDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public decimal TotalRevenue { get; set; }
}

public class MovieRevenueDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = null!;
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ShowtimeOccupancyDto
{
    public int ShowtimeId { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public decimal OccupancyPercentage { get; set; }
}

public class UserSpendingSummaryDto
{
    public int UserId { get; set; }
    public int TotalTicketsBought { get; set; }
    public decimal TotalAmountSpent { get; set; }
}