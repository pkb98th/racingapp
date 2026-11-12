using System.ComponentModel.DataAnnotations;
using RacingApp.Shared.Entities;

namespace RacingApp.Shared.Dtos;

public class InjuryDto
{
    public int Id { get; set; }
    public int RacerId { get; set; }
    public string RacerName { get; set; } = string.Empty;
    public DateTime DateReported { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? BodyPart { get; set; }
    public InjurySeverity Severity { get; set; }
    public InjuryStatus Status { get; set; }
    public string? VetName { get; set; }
    public List<TreatmentDto> Treatments { get; set; } = new();
}

public class CreateInjuryRequest
{
    [Required]
    public int RacerId { get; set; }
    [Required]
    public string Description { get; set; } = string.Empty;
    public string? BodyPart { get; set; }
    public InjurySeverity Severity { get; set; } = InjurySeverity.Minor;
    public InjuryStatus Status { get; set; } = InjuryStatus.UnderObservation;
}

public class TreatmentDto
{
    public int Id { get; set; }
    public int InjuryId { get; set; }
    public DateTime DateAdministered { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Medication { get; set; }
    public string? Dosage { get; set; }
    public int WithdrawalPeriodDays { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string? VetName { get; set; }
}

public class CreateTreatmentRequest
{
    [Required]
    public int InjuryId { get; set; }
    [Required]
    public DateTime DateAdministered { get; set; } = DateTime.UtcNow;
    [Required]
    public string Description { get; set; } = string.Empty;
    public string? Medication { get; set; }
    public string? Dosage { get; set; }
    public int WithdrawalPeriodDays { get; set; }
    public DateTime? FollowUpDate { get; set; }
}
