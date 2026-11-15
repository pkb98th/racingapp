using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/availabilities")]
[Authorize]
public class AvailabilitiesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AvailabilitiesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvailabilityDto>>> Get(
        [FromQuery] int? trackId,
        [FromQuery] bool? available)
    {
        var access = await AccessContext.LoadAsync(_db, User);

        var query = _db.RacerAvailabilities
            .Include(a => a.Racer).ThenInclude(r => r!.Stable)
            .Include(a => a.OwnerTeam)
            .AsQueryable();

        query = access.FilterReadable(query, _db);

        if (trackId.HasValue)
            query = query.Where(a => a.RacetrackId == trackId.Value);
        if (available.HasValue)
            query = query.Where(a => a.IsAvailable == available.Value);

        var list = await query.OrderBy(a => a.Racer!.Name).ToListAsync();
        return list.Select(a => a.ToDto()).ToList();
    }

    [HttpPut]
    [Authorize(Roles = $"{AppRoles.StableAdmin},{AppRoles.TrackAdmin}")]
    public async Task<IActionResult> Set(SetAvailabilityRequest request)
    {
        var racer = await _db.Racers.FindAsync(request.RacerId);
        if (racer is null)
            return NotFound(new { message = "Racer not found." });

        if (!await _db.Racetracks.AnyAsync(t => t.Id == request.RacetrackId))
            return NotFound(new { message = "Racetrack not found." });

        var access = await AccessContext.LoadAsync(_db, User);
        if (!await access.CanWriteAsync(_db, racer))
            return Forbid();

        var existing = await _db.RacerAvailabilities
            .FirstOrDefaultAsync(a => a.RacerId == request.RacerId && a.RacetrackId == request.RacetrackId);

        if (existing is null)
            _db.RacerAvailabilities.Add(new RacerAvailability
            {
                RacerId = request.RacerId,
                RacetrackId = request.RacetrackId,
                IsAvailable = request.IsAvailable,
                OwnerTeamId = racer.OwnerTeamId
            });
        else
            existing.IsAvailable = request.IsAvailable;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
