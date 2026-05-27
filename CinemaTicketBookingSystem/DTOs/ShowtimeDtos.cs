using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaTicketBookingSystem.WebApi.DTOs;

public class CreateShowtimeDto
{
    [Required]
    public int MovieId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int TheaterHallId { get; set; }

    [Required]
    [Range(0.01, 10000.00, ErrorMessage = "BasePrice must be greater than zero.")]
    public decimal BasePrice { get; set; }
}

public class PatchShowtimeDto
{
    public int? MovieId { get; set; }
    public DateTime? StartTime { get; set; }
    public int? TheaterHallId { get; set; }
    public decimal? BasePrice { get; set; }
}

public class ShowtimeResponseDto
{
    public int ShowtimeId { get; set; }
    public int? MovieId { get; set; }
    public string? MovieTitle { get; set; }
    public DateTime StartTime { get; set; }
    public int? TheaterHallId { get; set; }
    public string? TheaterHallName { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsDeleted { get; set; }
}

public class SeatStatusDto
{
    public string SeatNumber { get; set; } = null!;
    public string Row { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsAvailable { get; set; }
    public decimal Price { get; set; }
}