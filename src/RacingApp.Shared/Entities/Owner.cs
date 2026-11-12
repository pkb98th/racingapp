namespace RacingApp.Shared.Entities;

public class Owner
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AppUserId { get; set; }

    public ICollection<RacerOwnership> Ownerships { get; set; } = new List<RacerOwnership>();
}
