using System.ComponentModel.DataAnnotations;

namespace CinemaTicketBookingSystem.WebApi.DTOs;

public class CreateMovieDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes.")]
    public int DurationMinutes { get; set; }

    [Required]
    [StringLength(50)]
    public string Genre { get; set; } = null!;
}

public class UpdateMovieDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes.")]
    public int DurationMinutes { get; set; }

    [Required]
    [StringLength(50)]
    public string Genre { get; set; } = null!;
}

public class MovieResponseDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = null!;
    public int DurationMinutes { get; set; }
    public string Genre { get; set; } = null!;
    public bool IsDeleted { get; set; }
}
