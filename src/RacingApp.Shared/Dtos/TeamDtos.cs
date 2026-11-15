using System.ComponentModel.DataAnnotations;

namespace RacingApp.Shared.Dtos;

public class RecordAuditDto
{
    public Guid Key { get; set; }
    public int? OwnerTeamId { get; set; }
    public string? TeamName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CreatedByUserId { get; set; }
}

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? StableId { get; set; }
    public string? StableName { get; set; }
    public int? RacetrackId { get; set; }
    public string? RacetrackName { get; set; }
    public List<TeamMemberDto> Members { get; set; } = new();
}

public class TeamMemberDto
{
    public int PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateTeamRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    public int? StableId { get; set; }
    public int? RacetrackId { get; set; }
}

public class AddTeamMemberRequest
{
    [Required] public int PersonId { get; set; }
}

public class RecordShareDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityKey { get; set; }
    public int? ToTeamId { get; set; }
    public string? ToTeamName { get; set; }
    public int? ToPersonId { get; set; }
    public string? ToPersonName { get; set; }
    public SharePermission Permission { get; set; }
}

public class CreateShareRequest
{
    [Required] public string EntityType { get; set; } = string.Empty;
    [Required] public Guid EntityKey { get; set; }
    public int? ToTeamId { get; set; }
    public int? ToPersonId { get; set; }
    public SharePermission Permission { get; set; } = SharePermission.Read;
}
