using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaTicketBookingSystem.WebApi.DTOs;

public class CreateBookingDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int ShowtimeId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one seat must be selected.")]
    public List<string> SeatNumbers { get; set; } = new();
}

public class BookingReceiptDto
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public int ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public string TheaterHall { get; set; } = null!;
    public DateTime BookingTime { get; set; }
    public decimal TotalAmount { get; set; }
    public string BookingStatus { get; set; } = null!;
    public List<string> SeatNumbers { get; set; } = new();
}

public class PatchBookingStatusDto : IValidatableObject
{
    [StringLength(20)]
    public string? BookingStatus { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(BookingStatus))
        {
            yield return new ValidationResult(
                "BookingStatus is required.",
                new[] { nameof(BookingStatus) });
        }
    }
}
