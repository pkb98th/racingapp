using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/shares")]
[Authorize]
public class SharesController : ControllerBase
{
    private static readonly HashSet<string> ShareableEntities = new(StringComparer.Ordinal)
    {
        nameof(Racetrack), nameof(Stable), nameof(Racer), nameof(RacerOwnership),
        nameof(RacerAvailability), nameof(RaceMeeting), nameof(Race), nameof(RaceEntry),
        nameof(Injury), nameof(Treatment), nameof(Person)
    };

    private readonly AppDbContext _db;

    public SharesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecordShareDto>>> GetForEntity(
        [FromQuery] string entityType,
        [FromQuery] Guid entityKey)
    {
        if (!ShareableEntities.Contains(entityType))
            return BadRequest(new { message = "Unknown record type." });

        if (!await CanManageRecordAsync(entityType, entityKey))
            return Forbid();

        var shares = await _db.RecordShares
            .Include(s => s.ToTeam)
            .Include(s => s.ToPerson)
            .Where(s => s.EntityType == entityType && s.EntityKey == entityKey)
            .OrderBy(s => s.Id)
            .ToListAsync();

        return shares.Select(s => s.ToDto()).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<RecordShareDto>> Create(CreateShareRequest request)
    {
        if (!ShareableEntities.Contains(request.EntityType))
            return BadRequest(new { message = "Unknown record type." });

        if (request.ToTeamId is null && request.ToPersonId is null)
            return BadRequest(new { message = "A share must target a team or a user." });

        if (request.ToTeamId.HasValue && !await _db.Teams.AnyAsync(t => t.Id == request.ToTeamId))
            return BadRequest(new { message = "Unknown team." });
        if (request.ToPersonId.HasValue && !await _db.People.AnyAsync(p => p.Id == request.ToPersonId))
            return BadRequest(new { message = "Unknown user." });

        if (!await CanManageRecordAsync(request.EntityType, request.EntityKey))
            return Forbid();

        var duplicate = await _db.RecordShares.AnyAsync(s =>
            s.EntityType == request.EntityType
            && s.EntityKey == request.EntityKey
            && s.ToTeamId == request.ToTeamId
            && s.ToPersonId == request.ToPersonId);
        if (duplicate)
            return Conflict(new { message = "This record is already shared with that team or user." });

        var person = await CurrentUser.PersonAsync(_db, User);

        var share = new RecordShare
        {
            EntityType = request.EntityType,
            EntityKey = request.EntityKey,
            ToTeamId = request.ToTeamId,
            ToPersonId = request.ToPersonId,
            Permission = request.Permission,
            GrantedByPersonId = person?.Id,
            GrantedAtUtc = DateTime.UtcNow
        };
        _db.RecordShares.Add(share);
        await _db.SaveChangesAsync();

        share = await _db.RecordShares
            .Include(s => s.ToTeam)
            .Include(s => s.ToPerson)
            .FirstAsync(s => s.Id == share.Id);
        return Ok(share.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var share = await _db.RecordShares.FirstOrDefaultAsync(s => s.Id == id);
        if (share is null)
            return NotFound();

        if (!await CanManageRecordAsync(share.EntityType, share.EntityKey))
            return Forbid();

        _db.RecordShares.Remove(share);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Only the owning team (or an admin) may manage shares for a record.</summary>
    private async Task<bool> CanManageRecordAsync(string entityType, Guid entityKey)
    {
        if (User.IsInRole(AppRoles.TrackAdmin))
            return true;

        int? ownerTeamId = entityType switch
        {
            nameof(Racetrack) => (await _db.Racetracks.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(Stable) => (await _db.Stables.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(Racer) => (await _db.Racers.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(RacerOwnership) => (await _db.RacerOwnerships.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(RacerAvailability) => (await _db.RacerAvailabilities.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(RaceMeeting) => (await _db.RaceMeetings.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(Race) => (await _db.Races.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(RaceEntry) => (await _db.RaceEntries.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(Injury) => (await _db.Injuries.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(Treatment) => (await _db.Treatments.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            nameof(Person) => (await _db.People.FirstOrDefaultAsync(e => e.Key == entityKey))?.OwnerTeamId,
            _ => null
        };

        if (ownerTeamId is null)
            return false;

        var access = await AccessContext.LoadAsync(_db, User);
        return access.TeamIds.Contains(ownerTeamId.Value);
    }
}
