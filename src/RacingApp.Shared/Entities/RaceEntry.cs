namespace RacingApp.Shared.Entities;

public class RaceEntry
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public int RacerId { get; set; }
    public int? TrainerId { get; set; }
    public int? HandlerId { get; set; }
    public int TrapNumber { get; set; }
    public decimal? WeightKg { get; set; }
    public EntryStatus Status { get; set; } = EntryStatus.Entered;
    public int? FinishPosition { get; set; }
    public decimal? RaceTimeSeconds { get; set; }
    public string? Notes { get; set; }

    public Race Race { get; set; } = null!;
    public Racer Racer { get; set; } = null!;
    public Trainer? Trainer { get; set; }
    public Handler? Handler { get; set; }
}
