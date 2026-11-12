namespace RacingApp.Shared.Entities;

public class RaceMeeting
{
    public int Id { get; set; }
    public int RacetrackId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? GatesOpen { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;

    public Racetrack Racetrack { get; set; } = null!;
    public ICollection<Race> Races { get; set; } = new List<Race>();
}
