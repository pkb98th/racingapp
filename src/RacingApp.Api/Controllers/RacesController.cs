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
            .OrderBy(r => r.RaceNumber)
            .ToListAsync();
        return races.Select(r => r.ToDto()).ToList();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<RaceDto>> Create(CreateRaceRequest request)
    {
        var meeting = await _db.RaceMeetings.FindAsync(request.RaceMeetingId);
        if (meeting is null)
            return BadRequest(new { message = "Unknown meeting." });

        var race = new Race
        {
            RaceMeetingId = request.RaceMeetingId,
            RaceNumber = request.RaceNumber,
            DistanceMeters = request.DistanceMeters,
            Grade = request.Grade,
            PrizeMoney = request.PrizeMoney,
            ScheduledTime = request.ScheduledTime,
            Status = RaceStatus.Scheduled,
            OwnerTeamId = meeting.OwnerTeamId
        };
        _db.Races.Add(race);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByMeeting), new { meetingId = race.RaceMeetingId }, race.ToDto());
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
        var race = await _db.Races.FindAsync(id);
        if (race is null)
            return NotFound();

        _db.Races.Remove(race);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
