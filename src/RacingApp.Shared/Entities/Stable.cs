namespace RacingApp.Shared.Entities;

public class Stable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int Capacity { get; set; }

    public ICollection<Racer> Racers { get; set; } = new List<Racer>();
}
