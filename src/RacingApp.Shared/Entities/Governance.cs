namespace RacingApp.Shared.Entities;

public interface IManagedRecord
{
    int RecordStatus { get; set; }
    int? OwnerTeamId { get; set; }
    DateTime StartDate { get; set; }
    DateTime? EndDate { get; set; }
    string? RecordUserId { get; set; }
}

public enum TeamType
{
    Stable,
    Track,
    Other
}

[Flags]
public enum RecordRights
{
    Read = 1,
    Write = 2,
    All = Read | Write
}

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TeamType Type { get; set; }
    public int? StableId { get; set; }
    public int? RacetrackId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<TeamMember> Members { get; set; } = new();
}

public class TeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public RecordRights Rights { get; set; } = RecordRights.All;
    public Team? Team { get; set; }
}

public class RecordShare
{
    public int Id { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public int RecordId { get; set; }
    public int? TeamId { get; set; }
    public string? UserId { get; set; }
    public RecordRights Rights { get; set; } = RecordRights.Read;
    public Team? Team { get; set; }
}
