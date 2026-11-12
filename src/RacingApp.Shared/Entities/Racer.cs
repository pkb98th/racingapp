namespace RacingApp.Shared.Entities;

public class Racer
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
    public int? StableId { get; set; }
    public int? TrainerId { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public Stable? Stable { get; set; }
    public Trainer? Trainer { get; set; }
    public ICollection<RacerOwnership> Ownerships { get; set; } = new List<RacerOwnership>();
    public ICollection<RaceEntry> Entries { get; set; } = new List<RaceEntry>();
    public ICollection<Injury> Injuries { get; set; } = new List<Injury>();
}
