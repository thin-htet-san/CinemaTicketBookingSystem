using System.Collections.Generic;
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

    [StringLength(1000)]
    public string? Description { get; set; }
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

    [StringLength(1000)]
    public string? Description { get; set; }
}

public class MovieResponseDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = null!;
    public int DurationMinutes { get; set; }
    public string Genre { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public string? Description { get; set; }
}

public class PatchMovieDto : IValidatableObject
{
    [StringLength(200)]
    public string? Title { get; set; }

    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes.")]
    public int? DurationMinutes { get; set; }

    [StringLength(50)]
    public string? Genre { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Title == null && DurationMinutes == null && Genre == null && Description == null)
        {
            yield return new ValidationResult(
                "At least one field (Title, DurationMinutes, Genre, or Description) must be provided.",
                new[] { nameof(Title), nameof(DurationMinutes), nameof(Genre), nameof(Description) });
        }
    }
}
