using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaTicketBookingSystem.WebApi.DTOs;

public class CreateUserDto : IValidatableObject
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(PhoneNumber))
        {
            yield return new ValidationResult(
                "At least email or phone number must be provided.",
                new[] { nameof(Email), nameof(PhoneNumber) });
        }
    }
}

public class UpdateUserDto : IValidatableObject
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(PhoneNumber))
        {
            yield return new ValidationResult(
                "At least email or phone number must be provided.",
                new[] { nameof(Email), nameof(PhoneNumber) });
        }
    }
}

public class PatchUserDto : IValidatableObject
{
    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FullName == null && Email == null && PhoneNumber == null)
        {
            yield return new ValidationResult(
                "At least one field (FullName, Email, or PhoneNumber) must be provided.",
                new[] { nameof(FullName), nameof(Email), nameof(PhoneNumber) });
        }
    }
}

public class UserResponseDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
