using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/racetracks")]
[Authorize]
public class RacetracksController : ControllerBase
{
    private readonly AppDbContext _db;

    public RacetracksController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RacetrackDto>>> GetAll() =>
        await _db.Racetracks
            .Include(t => t.OwnerTeam)
            .OrderBy(t => t.Name)
            .Select(t => t.ToDto())
            .ToListAsync();

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<RacetrackDto>> Create(CreateRacetrackRequest request)
    {
        var access = await AccessContext.LoadAsync(_db, User);
        var ownerTeamId = await access.ResolveOwnerTeamIdAsync(_db, request.OwnerTeamId);
        if (ownerTeamId is null)
            return BadRequest(new { message = "No team available to own this record. Create a team first." });

        var track = new Racetrack
        {
            Name = request.Name,
            Address = request.Address,
            TrackLengthMeters = request.TrackLengthMeters,
            Surface = request.Surface,
            Capacity = request.Capacity,
            OwnerTeamId = ownerTeamId
        };
        _db.Racetracks.Add(track);
        await _db.SaveChangesAsync();

        var team = new Team { Name = $"{request.Name} Team", RacetrackId = track.Id };
        _db.Teams.Add(team);
        await _db.SaveChangesAsync();

        track = await _db.Racetracks.Include(t => t.OwnerTeam).FirstAsync(t => t.Id == track.Id);
        return CreatedAtAction(nameof(GetAll), track.ToDto());
    }
}
