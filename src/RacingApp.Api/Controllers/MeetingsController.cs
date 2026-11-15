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
            .Include(m => m.OwnerTeam)
            .OrderBy(m => m.Date)
            .ToListAsync();
        return meetings.Select(m => m.ToDto()).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RaceMeetingDto>> Get(int id)
    {
        var meeting = await LoadMeetingAsync(id);
        return meeting is null ? NotFound() : meeting.ToDto(includeRaces: true);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<RaceMeetingDto>> Create(CreateMeetingRequest request)
    {
        if (!await _db.Racetracks.AnyAsync(t => t.Id == request.RacetrackId))
            return BadRequest(new { message = "Unknown racetrack." });

        var access = await AccessContext.LoadAsync(_db, User);
        var ownerTeamId = await AccessContext.TeamForTrackAsync(_db, request.RacetrackId) is { } trackTeam
            ? trackTeam.Id
            : await access.ResolveOwnerTeamIdAsync(_db, request.OwnerTeamId);
        if (ownerTeamId is null)
            return BadRequest(new { message = "No team available to own this record. Create a team first." });

        var meeting = new RaceMeeting
        {
            RacetrackId = request.RacetrackId,
            Date = request.Date,
            GatesOpen = request.GatesOpen,
            Status = MeetingStatus.Scheduled,
            OwnerTeamId = ownerTeamId
        };
        _db.RaceMeetings.Add(meeting);
        await _db.SaveChangesAsync();

        var created = await LoadMeetingAsync(meeting.Id);
        return CreatedAtAction(nameof(Get), new { id = meeting.Id }, created!.ToDto());
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

    private async Task<RaceMeeting?> LoadMeetingAsync(int id) =>
        await _db.RaceMeetings
            .Include(m => m.Racetrack)
            .Include(m => m.Races).ThenInclude(r => r.Entries).ThenInclude(e => e.Racer).ThenInclude(r => r!.Stable)
            .FirstOrDefaultAsync(m => m.Id == id);
}
