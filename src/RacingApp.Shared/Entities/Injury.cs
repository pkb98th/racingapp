namespace RacingApp.Shared.Entities;

public class Injury
{
    public int Id { get; set; }
    public int RacerId { get; set; }
    public int? VetId { get; set; }
    public int? RaceEntryId { get; set; }
    public DateTime DateReported { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string? BodyPart { get; set; }
    public InjurySeverity Severity { get; set; } = InjurySeverity.Minor;
    public InjuryStatus Status { get; set; } = InjuryStatus.UnderObservation;

    public Racer Racer { get; set; } = null!;
    public Vet? Vet { get; set; }
    public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
}
