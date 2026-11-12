namespace RacingApp.Shared.Entities;

public class Treatment
{
    public int Id { get; set; }
    public int InjuryId { get; set; }
    public int? VetId { get; set; }
    public DateTime DateAdministered { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string? Medication { get; set; }
    public string? Dosage { get; set; }
    public int WithdrawalPeriodDays { get; set; }
    public DateTime? FollowUpDate { get; set; }

    public Injury Injury { get; set; } = null!;
    public Vet? Vet { get; set; }
}
