namespace RacingApp.Shared.Entities;

public class Handler
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int TrainerId { get; set; }

    public Trainer Trainer { get; set; } = null!;
}
