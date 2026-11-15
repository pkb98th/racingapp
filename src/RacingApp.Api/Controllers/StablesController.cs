using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/stables")]
[Authorize]
public class StablesController : ControllerBase
{
    private readonly AppDbContext _db;

    public StablesController(AppDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<IEnumerable<StableDto>>> GetAll() =>
        await _db.Stables
            .Include(s => s.OwnerTeam)
            .OrderBy(s => s.Name)
            .Select(s => s.ToDto())
            .ToListAsync();

    [HttpGet("mine")]
    public async Task<ActionResult<StableDto>> GetMine()
    {
        var person = await CurrentUser.PersonAsync(_db, User);
        if (person?.StableId is null)
            return NotFound();

        var stable = await _db.Stables
            .Include(s => s.OwnerTeam)
            .FirstOrDefaultAsync(s => s.Id == person.StableId.Value);
        return stable is null ? NotFound() : stable.ToDto();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<StableDto>> Create(CreateStableRequest request)
    {
        var access = await AccessContext.LoadAsync(_db, User);
        var ownerTeamId = await access.ResolveOwnerTeamIdAsync(_db, request.OwnerTeamId);
        if (ownerTeamId is null)
            return BadRequest(new { message = "No team available to own this record. Create a team first." });

        var stable = new Stable
        {
            Name = request.Name,
            Address = request.Address,
            Capacity = request.Capacity,
            OwnerTeamId = ownerTeamId
        };
        _db.Stables.Add(stable);
        await _db.SaveChangesAsync();

        var team = new Team { Name = $"{request.Name} Team", StableId = stable.Id };
        _db.Teams.Add(team);
        await _db.SaveChangesAsync();

        stable = await _db.Stables.Include(s => s.OwnerTeam).FirstAsync(s => s.Id == stable.Id);
        return CreatedAtAction(nameof(GetAll), stable.ToDto());
    }
}
