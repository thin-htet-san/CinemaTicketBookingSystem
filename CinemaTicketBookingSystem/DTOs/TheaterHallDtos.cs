using System.ComponentModel.DataAnnotations;

namespace CinemaTicketBookingSystem.WebApi.DTOs;

public class CreateTheaterHallDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CinemaBranchId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int TotalRows { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SeatsPerRow { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int CoupleSeatStartRow { get; set; }
}

public class UpdateTheaterHallDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CinemaBranchId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int TotalRows { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SeatsPerRow { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int CoupleSeatStartRow { get; set; }
}

public class TheaterHallResponseDto
{
    public int TheaterHallId { get; set; }
    public int CinemaBranchId { get; set; }
    public string CinemaBranchName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int TotalRows { get; set; }
    public int SeatsPerRow { get; set; }
    public int CoupleSeatStartRow { get; set; }
    public bool IsDeleted { get; set; }
}