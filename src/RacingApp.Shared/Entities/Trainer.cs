namespace RacingApp.Shared.Entities;

public class Trainer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? StableId { get; set; }
    public string? AppUserId { get; set; }

    public Stable? Stable { get; set; }
    public ICollection<Handler> Handlers { get; set; } = new List<Handler>();
    public ICollection<Racer> Racers { get; set; } = new List<Racer>();
}
