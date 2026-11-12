using System.ComponentModel.DataAnnotations;
using RacingApp.Shared.Entities;

namespace RacingApp.Shared.Dtos;

public class RacerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? KennelNumber { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public RacerSex Sex { get; set; }
    public string? Breed { get; set; }
    public string? Color { get; set; }
    public string? MicrochipNumber { get; set; }
    public decimal WeightKg { get; set; }
    public bool IsRetired { get; set; }
    public string? StableName { get; set; }
    public string? TrainerName { get; set; }
    public int AgeYears => DateOfBirth == default ? 0 : DateTime.UtcNow.Year - DateOfBirth.Year;
}

public class OwnerShareDto
{
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public decimal SharePercent { get; set; }
}

public class RacerDetailDto : RacerDto
{
    public List<OwnerShareDto> Owners { get; set; } = new();
    public List<InjuryDto> Injuries { get; set; } = new();
    public List<EntryDto> RecentEntries { get; set; } = new();
}

public class CreateRacerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? KennelNumber { get; set; }
    [Required]
    public DateOnly DateOfBirth { get; set; }
    public RacerSex Sex { get; set; }
    public string? Breed { get; set; }
    public string? Color { get; set; }
    public string? MicrochipNumber { get; set; }
    [Range(1, 100)]
    public decimal WeightKg { get; set; }
    public int? StableId { get; set; }
    public int? TrainerId { get; set; }
    public int? OwnerId { get; set; }
}

public class UpdateRacerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public string? Color { get; set; }
    [Range(1, 100)]
    public decimal WeightKg { get; set; }
    public bool IsRetired { get; set; }
    public int? StableId { get; set; }
    public int? TrainerId { get; set; }
}
