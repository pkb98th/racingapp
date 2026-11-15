using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RacingApp.Shared;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Data;

public sealed class AccessContext
{
    public int? PersonId { get; init; }
    public List<int> TeamIds { get; init; } = new();
    public bool IsTrackAdmin { get; init; }
    public bool IsVet { get; init; }
    public bool IsRacegoer { get; init; }

    /// <summary>Roles that are not restricted to their own teams when reading (admins, clinical, public).</summary>
    public bool ReadsEverything => IsTrackAdmin || IsVet || IsRacegoer;

    public static async Task<AccessContext> LoadAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var person = await CurrentUser.PersonAsync(db, user);
        var teamIds = person is null
            ? new List<int>()
            : await db.TeamMembers
                .Where(m => m.PersonId == person.Id)
                .Select(m => m.TeamId)
                .ToListAsync();

        return new AccessContext
        {
            PersonId = person?.Id,
            TeamIds = teamIds,
            IsTrackAdmin = user.IsInRole(AppRoles.TrackAdmin),
            IsVet = user.IsInRole(AppRoles.Vet),
            IsRacegoer = user.IsInRole(AppRoles.Racegoer)
        };
    }

    /// <summary>Filters records to those owned by the user's teams or shared to them.</summary>
    public IQueryable<T> FilterReadable<T>(IQueryable<T> query, AppDbContext db) where T : VersionedEntity
    {
        if (ReadsEverything)
            return query;

        var teamIds = TeamIds;
        var personId = PersonId ?? 0;
        var entityType = typeof(T).Name;

        return query.Where(e => teamIds.Contains(e.OwnerTeamId!.Value)
            || db.RecordShares.Any(s => s.EntityType == entityType
                && s.EntityKey == e.Key
                && ((s.ToTeamId != null && teamIds.Contains(s.ToTeamId.Value)) || s.ToPersonId == personId)));
    }

    public bool CanWrite(VersionedEntity entity) =>
        IsTrackAdmin || (entity.OwnerTeamId.HasValue && TeamIds.Contains(entity.OwnerTeamId.Value));

    public async Task<bool> CanWriteAsync(AppDbContext db, VersionedEntity entity)
    {
        if (CanWrite(entity))
            return true;

        if (PersonId is null)
            return false;

        var entityType = entity.GetType().Name;
        var teamIds = TeamIds;

        return await db.RecordShares.AnyAsync(s =>
            s.EntityType == entityType
            && s.EntityKey == entity.Key
            && (s.Permission == SharePermission.Write || s.Permission == SharePermission.All)
            && ((s.ToTeamId != null && teamIds.Contains(s.ToTeamId.Value)) || s.ToPersonId == PersonId));
    }

    /// <summary>Picks the team a new record belongs to: the requested team when allowed, otherwise the user's first team.</summary>
    public async Task<int?> ResolveOwnerTeamIdAsync(AppDbContext db, int? requestedTeamId)
    {
        if (requestedTeamId.HasValue)
        {
            var allowed = IsTrackAdmin || TeamIds.Contains(requestedTeamId.Value);
            if (allowed && await db.Teams.AnyAsync(t => t.Id == requestedTeamId.Value))
                return requestedTeamId;
        }

        return TeamIds.Count > 0 ? TeamIds[0] : null;
    }

    /// <summary>Team linked to a stable (its records' default owner).</summary>
    public static Task<Team?> TeamForStableAsync(AppDbContext db, int? stableId) =>
        stableId is null
            ? Task.FromResult<Team?>(null)
            : db.Teams.FirstOrDefaultAsync(t => t.StableId == stableId);

    /// <summary>Team linked to a racetrack (its records' default owner).</summary>
    public static Task<Team?> TeamForTrackAsync(AppDbContext db, int? racetrackId) =>
        racetrackId is null
            ? Task.FromResult<Team?>(null)
            : db.Teams.FirstOrDefaultAsync(t => t.RacetrackId == racetrackId);
}
