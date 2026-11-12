namespace RacingApp.Shared.Entities;

public class RacerOwnership
{
    public int Id { get; set; }
    public int RacerId { get; set; }
    public int OwnerId { get; set; }
    public decimal SharePercent { get; set; }
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? EndDate { get; set; }

    public Racer Racer { get; set; } = null!;
    public Owner Owner { get; set; } = null!;
}
