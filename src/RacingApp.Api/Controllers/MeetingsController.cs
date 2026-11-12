using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/meetings")]
[Authorize]
public class MeetingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MeetingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RaceMeetingDto>>> GetAll()
    {
        var meetings = await _db.RaceMeetings
            .Include(m => m.Racetrack)
            .Include(m => m.Races)
            .OrderBy(m => m.Date)
            .ToListAsync();
        return meetings.Select(m => m.ToDto()).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RaceMeetingDto>> Get(int id)
    {
        var meeting = await _db.RaceMeetings
            .Include(m => m.Racetrack)
            .Include(m => m.Races)
            .ThenInclude(r => r.Entries)
            .ThenInclude(e => e.Racer)
            .Include(m => m.Races)
            .ThenInclude(r => r.Entries)
            .ThenInclude(e => e.Trainer)
            .Include(m => m.Races)
            .ThenInclude(r => r.Entries)
            .ThenInclude(e => e.Handler)
            .FirstOrDefaultAsync(m => m.Id == id);

        return meeting is null ? NotFound() : meeting.ToDto(includeRaces: true);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<RaceMeetingDto>> Create(CreateMeetingRequest request)
    {
        var trackExists = await _db.Racetracks.AnyAsync(t => t.Id == request.RacetrackId);
        if (!trackExists)
            return BadRequest(new { message = "Unknown racetrack." });

        var meeting = new RaceMeeting
        {
            RacetrackId = request.RacetrackId,
            Date = request.Date,
            GatesOpen = request.GatesOpen,
            Status = MeetingStatus.Scheduled
        };
        _db.RaceMeetings.Add(meeting);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = meeting.Id }, meeting.ToDto());
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<IActionResult> SetStatus(int id, MeetingStatus status)
    {
        var meeting = await _db.RaceMeetings.FindAsync(id);
        if (meeting is null)
            return NotFound();

        meeting.Status = status;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
