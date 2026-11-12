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
        await _db.Racetracks.OrderBy(t => t.Name).Select(t => t.ToDto()).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RacetrackDto>> Get(int id)
    {
        var track = await _db.Racetracks.FindAsync(id);
        return track is null ? NotFound() : track.ToDto();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<RacetrackDto>> Create(CreateRacetrackRequest request)
    {
        var track = new Racetrack
        {
            Name = request.Name,
            Address = request.Address,
            TrackLengthMeters = request.TrackLengthMeters,
            Surface = request.Surface,
            Capacity = request.Capacity
        };
        _db.Racetracks.Add(track);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = track.Id }, track.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<IActionResult> Update(int id, CreateRacetrackRequest request)
    {
        var track = await _db.Racetracks.FindAsync(id);
        if (track is null)
            return NotFound();

        track.Name = request.Name;
        track.Address = request.Address;
        track.TrackLengthMeters = request.TrackLengthMeters;
        track.Surface = request.Surface;
        track.Capacity = request.Capacity;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var track = await _db.Racetracks.FindAsync(id);
        if (track is null)
            return NotFound();

        _db.Racetracks.Remove(track);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
