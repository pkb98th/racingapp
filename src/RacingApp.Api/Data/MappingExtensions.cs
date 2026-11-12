using RacingApp.Api.Models;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Data;

public static class MappingExtensions
{
    public static RacetrackDto ToDto(this Racetrack t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Address = t.Address,
        TrackLengthMeters = t.TrackLengthMeters,
        Surface = t.Surface,
        Capacity = t.Capacity
    };

    public static EntryDto ToDto(this RaceEntry e) => new()
    {
        Id = e.Id,
        RaceId = e.RaceId,
        RacerId = e.RacerId,
        RacerName = e.Racer?.Name ?? string.Empty,
        TrapNumber = e.TrapNumber,
        Status = e.Status,
        WeightKg = e.WeightKg,
        FinishPosition = e.FinishPosition,
        RaceTimeSeconds = e.RaceTimeSeconds,
        TrainerName = e.Trainer is null ? null : $"{e.Trainer.FirstName} {e.Trainer.LastName}",
        HandlerName = e.Handler is null ? null : $"{e.Handler.FirstName} {e.Handler.LastName}"
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
            : new List<EntryDto>()
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
            : new List<RaceDto>()
    };

    public static OwnerShareDto ToDto(this RacerOwnership o) => new()
    {
        OwnerId = o.OwnerId,
        OwnerName = o.Owner is null ? string.Empty : $"{o.Owner.FirstName} {o.Owner.LastName}",
        SharePercent = o.SharePercent
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
        VetName = i.Vet is null ? null : $"{i.Vet.FirstName} {i.Vet.LastName}",
        Treatments = i.Treatments
            .OrderByDescending(t => t.DateAdministered)
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
        VetName = t.Vet is null ? null : $"{t.Vet.FirstName} {t.Vet.LastName}"
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
        StableName = r.Stable?.Name,
        TrainerName = r.Trainer is null ? null : $"{r.Trainer.FirstName} {r.Trainer.LastName}"
    };

    public static OwnerDto ToDto(this Owner o) => new()
    {
        Id = o.Id,
        FirstName = o.FirstName,
        LastName = o.LastName,
        FullName = $"{o.FirstName} {o.LastName}",
        Email = o.Email,
        Phone = o.Phone
    };

    public static TrainerDto ToDto(this Trainer t) => new()
    {
        Id = t.Id,
        FirstName = t.FirstName,
        LastName = t.LastName,
        FullName = $"{t.FirstName} {t.LastName}",
        LicenseNumber = t.LicenseNumber,
        Email = t.Email,
        Phone = t.Phone,
        StableName = t.Stable?.Name
    };

    public static VetDto ToDto(this Vet v) => new()
    {
        Id = v.Id,
        FirstName = v.FirstName,
        LastName = v.LastName,
        FullName = $"{v.FirstName} {v.LastName}",
        LicenseNumber = v.LicenseNumber,
        ClinicName = v.ClinicName,
        Phone = v.Phone
    };

    public static HandlerDto ToDto(this Handler h) => new()
    {
        Id = h.Id,
        FirstName = h.FirstName,
        LastName = h.LastName,
        FullName = $"{h.FirstName} {h.LastName}",
        Phone = h.Phone,
        TrainerId = h.TrainerId,
        TrainerName = h.Trainer is null ? string.Empty : $"{h.Trainer.FirstName} {h.Trainer.LastName}"
    };

    public static StableDto ToDto(this Stable s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Address = s.Address,
        Capacity = s.Capacity
    };

    public static string DisplayName(this ApplicationUser user) =>
        string.IsNullOrWhiteSpace(user.FullName) ? user.UserName ?? user.Email ?? user.Id : user.FullName;
}
