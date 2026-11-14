namespace RacingApp.Shared.Entities;

public class Injury
{
    public int Id { get; set; }
    public int RacerId { get; set; }
    public DateTime DateReported { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? BodyPart { get; set; }
    public InjurySeverity Severity { get; set; }
    public InjuryStatus Status { get; set; }
    public int? ReportedByPersonId { get; set; }

    public Racer? Racer { get; set; }
    public Person? ReportedBy { get; set; }
    public List<Treatment> Treatments { get; set; } = new();
}

public class Treatment
{
    public int Id { get; set; }
    public int InjuryId { get; set; }
    public DateTime DateAdministered { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Medication { get; set; }
    public string? Dosage { get; set; }
    public int WithdrawalPeriodDays { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public int? AdministeredByPersonId { get; set; }

    public Injury? Injury { get; set; }
    public Person? AdministeredBy { get; set; }
}
