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
    [Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.Trainer}")]
    public async Task<ActionResult<EntryDto>> Create(CreateEntryRequest request)
    {
        var race = await _db.Races.FindAsync(request.RaceId);
        if (race is null)
            return BadRequest(new { message = "Unknown race." });

        var racer = await _db.Racers.FindAsync(request.RacerId);
        if (racer is null)
            return BadRequest(new { message = "Unknown racer." });

        var duplicate = await _db.RaceEntries.AnyAsync(e => e.RaceId == request.RaceId && e.RacerId == request.RacerId);
        if (duplicate)
            return Conflict(new { message = "This racer is already entered in the race." });

        var trapTaken = await _db.RaceEntries.AnyAsync(e => e.RaceId == request.RaceId && e.TrapNumber == request.TrapNumber);
        if (trapTaken)
            return Conflict(new { message = "This trap number is already taken." });

        var entry = new RaceEntry
        {
            RaceId = request.RaceId,
            RacerId = request.RacerId,
            TrainerId = request.TrainerId,
            HandlerId = request.HandlerId,
            TrapNumber = request.TrapNumber,
            WeightKg = request.WeightKg
        };
        entry.Racer = racer;
        _db.RaceEntries.Add(entry);
        await _db.SaveChangesAsync();
        return Ok(entry.ToDto());
    }

    [HttpPut("{id:int}/result")]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<IActionResult> SetResult(int id, SetResultRequest request)
    {
        var entry = await _db.RaceEntries.Include(e => e.Race).FirstOrDefaultAsync(e => e.Id == id);
        if (entry is null)
            return NotFound();

        entry.FinishPosition = request.FinishPosition;
        entry.RaceTimeSeconds = request.RaceTimeSeconds;
        entry.Status = EntryStatus.Resulted;
        entry.Race.Status = RaceStatus.Resulted;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/scratch")]
    [Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.Trainer}")]
    public async Task<IActionResult> Scratch(int id)
    {
        var entry = await _db.RaceEntries.FindAsync(id);
        if (entry is null)
            return NotFound();

        entry.Status = EntryStatus.Scratched;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
