using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TeamsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetAll()
    {
        var teams = await _db.Teams
            .Include(t => t.Stable)
            .Include(t => t.Racetrack)
            .Include(t => t.Members).ThenInclude(m => m.Person)
            .OrderBy(t => t.Name)
            .ToListAsync();
        return teams.Select(t => t.ToDto()).ToList();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<TeamDto>> Create(CreateTeamRequest request)
    {
        if (request.StableId is null && request.RacetrackId is null)
            return BadRequest(new { message = "A team must be linked to a stable or a racetrack." });

        if (request.StableId.HasValue && !await _db.Stables.AnyAsync(s => s.Id == request.StableId))
            return BadRequest(new { message = "Unknown stable." });
        if (request.RacetrackId.HasValue && !await _db.Racetracks.AnyAsync(t => t.Id == request.RacetrackId))
            return BadRequest(new { message = "Unknown racetrack." });

        var team = new Team
        {
            Name = request.Name,
            StableId = request.StableId,
            RacetrackId = request.RacetrackId
        };
        _db.Teams.Add(team);
        await _db.SaveChangesAsync();

        team = await LoadTeamAsync(team.Id);
        return CreatedAtAction(nameof(GetAll), team!.ToDto());
    }

    [HttpPost("{id:int}/members")]
    [Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.StableAdmin}")]
    public async Task<IActionResult> AddMember(int id, AddTeamMemberRequest request)
    {
        if (!await CanManageTeamAsync(id))
            return Forbid();

        var teamExists = await _db.Teams.AnyAsync(t => t.Id == id);
        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == request.PersonId);
        if (!teamExists || person is null)
            return NotFound(new { message = "Team or person not found." });

        var alreadyMember = await _db.TeamMembers.AnyAsync(m => m.TeamId == id && m.PersonId == request.PersonId);
        if (alreadyMember)
            return Conflict(new { message = "This person is already a member of the team." });

        _db.TeamMembers.Add(new TeamMember { TeamId = id, PersonId = request.PersonId });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}/members/{personId:int}")]
    [Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.StableAdmin}")]
    public async Task<IActionResult> RemoveMember(int id, int personId)
    {
        if (!await CanManageTeamAsync(id))
            return Forbid();

        var member = await _db.TeamMembers.FirstOrDefaultAsync(m => m.TeamId == id && m.PersonId == personId);
        if (member is null)
            return NotFound();

        _db.TeamMembers.Remove(member);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> CanManageTeamAsync(int teamId)
    {
        if (User.IsInRole(AppRoles.TrackAdmin))
            return true;

        if (!User.IsInRole(AppRoles.StableAdmin))
            return false;

        var access = await AccessContext.LoadAsync(_db, User);
        return access.TeamIds.Contains(teamId);
    }

    private async Task<Team?> LoadTeamAsync(int id) =>
        await _db.Teams
            .Include(t => t.Stable)
            .Include(t => t.Racetrack)
            .Include(t => t.Members).ThenInclude(m => m.Person)
            .FirstOrDefaultAsync(t => t.Id == id);
}
