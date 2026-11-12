namespace RacingApp.Shared;

public enum RacerSex
{
    Male,
    Female
}

public enum MeetingStatus
{
    Scheduled,
    Open,
    InProgress,
    Completed,
    Cancelled
}

public enum RaceStatus
{
    Scheduled,
    WeighIn,
    InProgress,
    Resulted,
    Void
}

public enum EntryStatus
{
    Entered,
    Scratched,
    Withdrawn,
    Resulted
}

public enum InjurySeverity
{
    Minor,
    Moderate,
    Severe
}

public enum InjuryStatus
{
    UnderObservation,
    InTreatment,
    Recovering,
    Cleared
}
