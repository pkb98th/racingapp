using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/races")]
[Authorize]
public class RacesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RacesController(AppDbContext db) => _db = db;

    [HttpGet("meeting/{meetingId:int}")]
    public async Task<ActionResult<IEnumerable<RaceDto>>> GetByMeeting(int meetingId)
    {
        var races = await _db.Races
            .Where(r => r.RaceMeetingId == meetingId)
            .Include(r => r.Entries)
            .ThenInclude(e => e.Racer)
            .Include(r => r.Entries)
            .ThenInclude(e => e.Trainer)
            .Include(r => r.Entries)
            .ThenInclude(e => e.Handler)
            .OrderBy(r => r.RaceNumber)
            .ToListAsync();

        return races.Select(r => r.ToDto(includeEntries: true)).ToList();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<RaceDto>> Create(CreateRaceRequest request)
    {
        var meetingExists = await _db.RaceMeetings.AnyAsync(m => m.Id == request.RaceMeetingId);
        if (!meetingExists)
            return BadRequest(new { message = "Unknown race meeting." });

        var duplicate = await _db.Races.AnyAsync(r =>
            r.RaceMeetingId == request.RaceMeetingId && r.RaceNumber == request.RaceNumber);
        if (duplicate)
            return Conflict(new { message = "A race with this number already exists in the meeting." });

        var race = new Race
        {
            RaceMeetingId = request.RaceMeetingId,
            RaceNumber = request.RaceNumber,
            DistanceMeters = request.DistanceMeters,
            Grade = request.Grade,
            PrizeMoney = request.PrizeMoney,
            ScheduledTime = request.ScheduledTime
        };
        _db.Races.Add(race);
        await _db.SaveChangesAsync();
        return Ok(race.ToDto());
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<IActionResult> SetStatus(int id, RaceStatus status)
    {
        var race = await _db.Races.FindAsync(id);
        if (race is null)
            return NotFound();

        race.Status = status;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var race = await _db.Races.Include(r => r.Entries).FirstOrDefaultAsync(r => r.Id == id);
        if (race is null)
            return NotFound();

        _db.Races.Remove(race);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
