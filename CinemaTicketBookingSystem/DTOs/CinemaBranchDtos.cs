using System.ComponentModel.DataAnnotations;

namespace CinemaTicketBookingSystem.WebApi.DTOs;

public class CreateCinemaBranchDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Location { get; set; }
}

public class UpdateCinemaBranchDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Location { get; set; }
}

public class CinemaBranchResponseDto
{
    public int CinemaBranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
    public bool IsDeleted { get; set; }
}