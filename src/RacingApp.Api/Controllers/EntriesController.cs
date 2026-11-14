using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/entries")]
[Authorize]
public class EntriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EntriesController(AppDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<EntryDto>> Create(CreateEntryRequest request)
    {
        var race = await _db.Races
            .Include(r => r.RaceMeeting)
            .FirstOrDefaultAsync(r => r.Id == request.RaceId);
        if (race is null)
            return BadRequest(new { message = "Unknown race." });

        if (race.Status != RaceStatus.Scheduled)
            return BadRequest(new { message = "Entries are only allowed while the race is scheduled." });

        var racer = await _db.Racers.FindAsync(request.RacerId);
        if (racer is null)
            return BadRequest(new { message = "Unknown racer." });

        if (racer.IsRetired)
            return BadRequest(new { message = "This racer is retired." });

        var alreadyEntered = await _db.RaceEntries
            .AnyAsync(e => e.RaceId == request.RaceId && e.RacerId == request.RacerId);
        if (alreadyEntered)
            return Conflict(new { message = "This racer is already entered in the race." });

        var trackId = race.RaceMeeting!.RacetrackId;
        var isAvailable = await _db.RacerAvailabilities
            .AnyAsync(a => a.RacerId == request.RacerId
                && a.RacetrackId == trackId
                && a.IsAvailable);

        if (!isAvailable)
            return BadRequest(new { message = "The stable has not flagged this racer as available for this track." });

        var duplicateTrap = await _db.RaceEntries
            .AnyAsync(e => e.RaceId == request.RaceId && e.TrapNumber == request.TrapNumber);
        if (duplicateTrap)
            return Conflict(new { message = "That trap number is already taken in this race." });

        var entry = new RaceEntry
        {
            RaceId = request.RaceId,
            RacerId = request.RacerId,
            TrapNumber = request.TrapNumber,
            WeightKg = request.WeightKg ?? racer.WeightKg,
            Status = EntryStatus.Entered
        };

        _db.RaceEntries.Add(entry);
        await _db.SaveChangesAsync();

        entry.Racer = await _db.Racers.Include(r => r.Stable).FirstAsync(r => r.Id == entry.RacerId);
        return CreatedAtAction(nameof(Get), new { id = entry.Id }, entry.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EntryDto>> Get(int id)
    {
        var entry = await _db.RaceEntries
            .Include(e => e.Racer).ThenInclude(r => r!.Stable)
            .FirstOrDefaultAsync(e => e.Id == id);
        return entry is null ? NotFound() : entry.ToDto();
    }

    [HttpPut("{id:int}/result")]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<IActionResult> SetResult(int id, SetResultRequest request)
    {
        var entry = await _db.RaceEntries.FindAsync(id);
        if (entry is null)
            return NotFound();

        entry.FinishPosition = request.FinishPosition;
        entry.RaceTimeSeconds = request.RaceTimeSeconds;
        entry.Status = EntryStatus.Ran;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/scratch")]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<IActionResult> Scratch(int id)
    {
        var entry = await _db.RaceEntries.FindAsync(id);
        if (entry is null)
            return NotFound();

        entry.Status = EntryStatus.Scratched;
        entry.FinishPosition = null;
        entry.RaceTimeSeconds = null;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
