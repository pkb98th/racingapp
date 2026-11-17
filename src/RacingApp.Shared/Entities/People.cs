namespace RacingApp.Shared.Entities;

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? LicenseNumber { get; set; }
    public int? StableId { get; set; }
    public string? AppUserId { get; set; }

    public Stable? Stable { get; set; }

    public string DisplayName => $"{FirstName} {LastName}".Trim();
}

public class Stable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int Capacity { get; set; }
}
