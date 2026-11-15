namespace RacingApp.Shared.Entities;

/// <summary>
/// Base for all domain records. Every row is a version: the current version has
/// EndDate = null; when a record is updated the original row is copied and closed
/// (EndDate set) while the current row keeps its Id so references stay stable.
/// </summary>
public abstract class VersionedEntity
{
    /// <summary>Logical identity, shared by all versions of the same record.</summary>
    public Guid Key { get; set; }

    /// <summary>Team that owns the record. Records are only accessible to this team unless shared.</summary>
    public int? OwnerTeamId { get; set; }
    public Team? OwnerTeam { get; set; }

    /// <summary>UTC date and time this version was created (set automatically on insert).</summary>
    public DateTime StartDate { get; set; }

    /// <summary>UTC date and time this version was superseded; null while current.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Identity user id that created this version (set automatically on insert).</summary>
    public string? CreatedByUserId { get; set; }
}
