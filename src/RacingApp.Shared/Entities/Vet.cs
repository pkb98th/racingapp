namespace RacingApp.Shared.Entities;

public class Vet
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? ClinicName { get; set; }
    public string? Phone { get; set; }
    public string? AppUserId { get; set; }

    public ICollection<Injury> Injuries { get; set; } = new List<Injury>();
    public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
}
