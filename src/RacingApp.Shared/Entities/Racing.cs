namespace RacingApp.Shared.Entities;

public class Racetrack
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int TrackLengthMeters { get; set; }
    public string? Surface { get; set; }
    public int Capacity { get; set; }
}

public class Racer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? KennelNumber { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public RacerSex Sex { get; set; }
    public string? Breed { get; set; }
    public string? Color { get; set; }
    public string? MicrochipNumber { get; set; }
    public decimal WeightKg { get; set; }
    public bool IsRetired { get; set; }

    public int StableId { get; set; }
    public int? TrainerPersonId { get; set; }
    public int? HandlerPersonId { get; set; }

    public Stable? Stable { get; set; }
    public Person? Trainer { get; set; }
    public Person? Handler { get; set; }
    public List<RacerOwnership> Ownerships { get; set; } = new();
    public List<RacerAvailability> Availabilities { get; set; } = new();
}

public class RacerOwnership
{
    public int Id { get; set; }
    public int RacerId { get; set; }
    public int OwnerPersonId { get; set; }
    public decimal SharePercent { get; set; }

    public Racer? Racer { get; set; }
    public Person? Owner { get; set; }
}

public class RacerAvailability
{
    public int Id { get; set; }
    public int RacerId { get; set; }
    public int RacetrackId { get; set; }
    public bool IsAvailable { get; set; }

    public Racer? Racer { get; set; }
    public Racetrack? Racetrack { get; set; }
}

public class RaceMeeting
{
    public int Id { get; set; }
    public int RacetrackId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? GatesOpen { get; set; }
    public MeetingStatus Status { get; set; }

    public Racetrack? Racetrack { get; set; }
    public List<Race> Races { get; set; } = new();
}

public class Race
{
    public int Id { get; set; }
    public int RaceMeetingId { get; set; }
    public int RaceNumber { get; set; }
    public int DistanceMeters { get; set; }
    public string? Grade { get; set; }
    public decimal PrizeMoney { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public RaceStatus Status { get; set; }

    public RaceMeeting? RaceMeeting { get; set; }
    public List<RaceEntry> Entries { get; set; } = new();
}

public class RaceEntry
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public int RacerId { get; set; }
    public int TrapNumber { get; set; }
    public EntryStatus Status { get; set; }
    public decimal? WeightKg { get; set; }
    public int? FinishPosition { get; set; }
    public decimal? RaceTimeSeconds { get; set; }

    public Race? Race { get; set; }
    public Racer? Racer { get; set; }
}
