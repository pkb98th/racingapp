using System.ComponentModel.DataAnnotations;

namespace RacingApp.Shared.Dtos;

public class RacetrackDto : RecordAuditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int TrackLengthMeters { get; set; }
    public string? Surface { get; set; }
    public int Capacity { get; set; }
}

public class CreateRacetrackRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    [Range(50, 5000)] public int TrackLengthMeters { get; set; } = 400;
    public string? Surface { get; set; }
    [Range(1, 200000)] public int Capacity { get; set; } = 1000;
    public int? OwnerTeamId { get; set; }
}

public class StableDto : RecordAuditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int Capacity { get; set; }
}

public class CreateStableRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    [Range(1, 500)] public int Capacity { get; set; } = 20;
    public int? OwnerTeamId { get; set; }
}

public class RaceMeetingDto : RecordAuditDto
{
    public int Id { get; set; }
    public int RacetrackId { get; set; }
    public string RacetrackName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly? GatesOpen { get; set; }
    public MeetingStatus Status { get; set; }
    public List<RaceDto> Races { get; set; } = new();
}

public class CreateMeetingRequest
{
    [Required] public int RacetrackId { get; set; }
    [Required] public DateOnly Date { get; set; }
    public TimeOnly? GatesOpen { get; set; }
    public int? OwnerTeamId { get; set; }
}

public class RaceDto : RecordAuditDto
{
    public int Id { get; set; }
    public int RaceMeetingId { get; set; }
    public int RaceNumber { get; set; }
    public int DistanceMeters { get; set; }
    public string? Grade { get; set; }
    public decimal PrizeMoney { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public RaceStatus Status { get; set; }
    public List<EntryDto> Entries { get; set; } = new();
}

public class CreateRaceRequest
{
    [Required] public int RaceMeetingId { get; set; }
    [Range(1, 99)] public int RaceNumber { get; set; }
    [Range(50, 5000)] public int DistanceMeters { get; set; }
    public string? Grade { get; set; }
    public decimal PrizeMoney { get; set; }
    [Required] public TimeOnly ScheduledTime { get; set; }
}

public class EntryDto : RecordAuditDto
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public int RacerId { get; set; }
    public string RacerName { get; set; } = string.Empty;
    public string? StableName { get; set; }
    public int TrapNumber { get; set; }
    public EntryStatus Status { get; set; }
    public decimal? WeightKg { get; set; }
    public int? FinishPosition { get; set; }
    public decimal? RaceTimeSeconds { get; set; }
}

public class CreateEntryRequest
{
    [Required] public int RaceId { get; set; }
    [Required] public int RacerId { get; set; }
    [Range(1, 12)] public int TrapNumber { get; set; }
    public decimal? WeightKg { get; set; }
}

public class SetResultRequest
{
    [Range(1, 12)] public int FinishPosition { get; set; }
    public decimal? RaceTimeSeconds { get; set; }
}

public class AvailabilityDto : RecordAuditDto
{
    public int RacerId { get; set; }
    public string RacerName { get; set; } = string.Empty;
    public string? StableName { get; set; }
    public int RacetrackId { get; set; }
    public bool IsAvailable { get; set; }
}

public class SetAvailabilityRequest
{
    [Required] public int RacerId { get; set; }
    [Required] public int RacetrackId { get; set; }
    public bool IsAvailable { get; set; }
}
