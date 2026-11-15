namespace RacingApp.Shared.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? StableId { get; set; }
    public int? RacetrackId { get; set; }

    public Stable? Stable { get; set; }
    public Racetrack? Racetrack { get; set; }
    public List<TeamMember> Members { get; set; } = new();
}

public class TeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int PersonId { get; set; }

    public Team? Team { get; set; }
    public Person? Person { get; set; }
}

public class RecordShare
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityKey { get; set; }
    public int? ToTeamId { get; set; }
    public int? ToPersonId { get; set; }
    public SharePermission Permission { get; set; }
    public int? GrantedByPersonId { get; set; }
    public DateTime GrantedAtUtc { get; set; }

    public Team? ToTeam { get; set; }
    public Person? ToPerson { get; set; }
    public Person? GrantedBy { get; set; }
}
