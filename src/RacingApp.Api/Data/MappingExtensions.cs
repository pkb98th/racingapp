using RacingApp.Api.Models;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Data;

public static class MappingExtensions
{
    public static string DisplayName(this ApplicationUser user) =>
        string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? "User" : user.FullName;

    public static RacetrackDto ToDto(this Racetrack t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Address = t.Address,
        TrackLengthMeters = t.TrackLengthMeters,
        Surface = t.Surface,
        Capacity = t.Capacity
    };

    public static StableDto ToDto(this Stable s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Address = s.Address,
        Capacity = s.Capacity
    };

    public static EntryDto ToDto(this RaceEntry e) => new()
    {
        Id = e.Id,
        RaceId = e.RaceId,
        RacerId = e.RacerId,
        RacerName = e.Racer?.Name ?? string.Empty,
        StableName = e.Racer?.Stable?.Name,
        TrapNumber = e.TrapNumber,
        Status = e.Status,
        WeightKg = e.WeightKg,
        FinishPosition = e.FinishPosition,
        RaceTimeSeconds = e.RaceTimeSeconds
    };

    public static RaceDto ToDto(this Race r, bool includeEntries = false) => new()
    {
        Id = r.Id,
        RaceMeetingId = r.RaceMeetingId,
        RaceNumber = r.RaceNumber,
        DistanceMeters = r.DistanceMeters,
        Grade = r.Grade,
        PrizeMoney = r.PrizeMoney,
        ScheduledTime = r.ScheduledTime,
        Status = r.Status,
        Entries = includeEntries
            ? r.Entries.OrderBy(e => e.TrapNumber).Select(e => e.ToDto()).ToList()
            : new()
    };

    public static RaceMeetingDto ToDto(this RaceMeeting m, bool includeRaces = false) => new()
    {
        Id = m.Id,
        RacetrackId = m.RacetrackId,
        RacetrackName = m.Racetrack?.Name ?? string.Empty,
        Date = m.Date,
        GatesOpen = m.GatesOpen,
        Status = m.Status,
        Races = includeRaces
            ? m.Races.OrderBy(r => r.RaceNumber).Select(r => r.ToDto(includeEntries: true)).ToList()
            : new()
    };

    public static OwnerShareDto ToDto(this RacerOwnership o) => new()
    {
        OwnerPersonId = o.OwnerPersonId,
        OwnerName = o.Owner?.DisplayName ?? string.Empty,
        SharePercent = o.SharePercent
    };

    public static AvailabilityDto ToDto(this RacerAvailability a) => new()
    {
        RacerId = a.RacerId,
        RacerName = a.Racer?.Name ?? string.Empty,
        StableName = a.Racer?.Stable?.Name,
        RacetrackId = a.RacetrackId,
        IsAvailable = a.IsAvailable
    };

    public static RacerDto ToDto(this Racer r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        KennelNumber = r.KennelNumber,
        DateOfBirth = r.DateOfBirth,
        Sex = r.Sex,
        Breed = r.Breed,
        Color = r.Color,
        MicrochipNumber = r.MicrochipNumber,
        WeightKg = r.WeightKg,
        IsRetired = r.IsRetired,
        StableId = r.StableId,
        StableName = r.Stable?.Name,
        TrainerName = r.Trainer?.DisplayName,
        HandlerName = r.Handler?.DisplayName
    };

    public static RacerDetailDto ToDetailDto(this Racer r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        KennelNumber = r.KennelNumber,
        DateOfBirth = r.DateOfBirth,
        Sex = r.Sex,
        Breed = r.Breed,
        Color = r.Color,
        MicrochipNumber = r.MicrochipNumber,
        WeightKg = r.WeightKg,
        IsRetired = r.IsRetired,
        StableId = r.StableId,
        StableName = r.Stable?.Name,
        TrainerName = r.Trainer?.DisplayName,
        HandlerName = r.Handler?.DisplayName,
        Owners = r.Ownerships.Select(o => o.ToDto()).ToList(),
        Availabilities = r.Availabilities
            .OrderBy(a => a.RacetrackId)
            .Select(a => a.ToDto())
            .ToList(),
        Injuries = new(),
        RecentEntries = new()
    };

    public static InjuryDto ToDto(this Injury i) => new()
    {
        Id = i.Id,
        RacerId = i.RacerId,
        RacerName = i.Racer?.Name ?? string.Empty,
        DateReported = i.DateReported,
        Description = i.Description,
        BodyPart = i.BodyPart,
        Severity = i.Severity,
        Status = i.Status,
        ReportedByName = i.ReportedBy?.DisplayName,
        Treatments = i.Treatments
            .OrderBy(t => t.DateAdministered)
            .Select(t => t.ToDto())
            .ToList()
    };

    public static TreatmentDto ToDto(this Treatment t) => new()
    {
        Id = t.Id,
        InjuryId = t.InjuryId,
        DateAdministered = t.DateAdministered,
        Description = t.Description,
        Medication = t.Medication,
        Dosage = t.Dosage,
        WithdrawalPeriodDays = t.WithdrawalPeriodDays,
        FollowUpDate = t.FollowUpDate,
        AdministeredByName = t.AdministeredBy?.DisplayName
    };
}
