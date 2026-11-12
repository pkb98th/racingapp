namespace RacingApp.Shared.Entities;

public class Race
{
    public int Id { get; set; }
    public int RaceMeetingId { get; set; }
    public int RaceNumber { get; set; }
    public int DistanceMeters { get; set; }
    public string? Grade { get; set; }
    public decimal PrizeMoney { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public RaceStatus Status { get; set; } = RaceStatus.Scheduled;

    public RaceMeeting RaceMeeting { get; set; } = null!;
    public ICollection<RaceEntry> Entries { get; set; } = new List<RaceEntry>();
}
