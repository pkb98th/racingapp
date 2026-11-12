namespace RacingApp.Shared.Entities;

public class Racetrack
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int TrackLengthMeters { get; set; }
    public string? Surface { get; set; }
    public int Capacity { get; set; }

    public ICollection<RaceMeeting> Meetings { get; set; } = new List<RaceMeeting>();
}
