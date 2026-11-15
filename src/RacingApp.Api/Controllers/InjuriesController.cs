using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/injuries")]
[Authorize]
public class InjuriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public InjuriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InjuryDto>>> GetAll([FromQuery] int? racerId)
    {
        if (User.IsInRole(AppRoles.Racegoer) && !User.IsInRole(AppRoles.TrackAdmin))
            return Forbid();

        var access = await AccessContext.LoadAsync(_db, User);

        var query = _db.Injuries
            .Include(i => i.Racer)
            .Include(i => i.ReportedBy)
            .Include(i => i.OwnerTeam)
            .Include(i => i.Treatments).ThenInclude(t => t.AdministeredBy)
            .AsQueryable();

        query = access.FilterReadable(query, _db);

        if (racerId.HasValue)
            query = query.Where(i => i.RacerId == racerId.Value);

        var injuries = await query.OrderByDescending(i => i.DateReported).ToListAsync();
        return injuries.Select(i => i.ToDto()).ToList();
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.Vet},{AppRoles.Trainer},{AppRoles.StableAdmin}")]
    public async Task<ActionResult<InjuryDto>> Create(CreateInjuryRequest request)
    {
        var person = await CurrentUser.PersonAsync(_db, User);

        var racer = await _db.Racers.AsNoTracking().FirstOrDefaultAsync(r => r.Id == request.RacerId);
        if (racer is null)
            return BadRequest(new { message = "Unknown racer." });

        var injury = new Injury
        {
            RacerId = request.RacerId,
            DateReported = DateTime.UtcNow,
            Description = request.Description,
            BodyPart = request.BodyPart,
            Severity = request.Severity,
            Status = InjuryStatus.UnderObservation,
            ReportedByPersonId = person?.Id,
            OwnerTeamId = racer.OwnerTeamId
        };

        _db.Injuries.Add(injury);
        await _db.SaveChangesAsync();

        injury.Racer = await _db.Racers.Include(r => r.OwnerTeam).FirstAsync(r => r.Id == injury.RacerId);
        return CreatedAtAction(nameof(GetAll), injury.ToDto());
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = $"{AppRoles.Vet},{AppRoles.TrackAdmin}")]
    public async Task<IActionResult> SetStatus(int id, InjuryStatus status)
    {
        var injury = await _db.Injuries.FindAsync(id);
        if (injury is null)
            return NotFound();

        injury.Status = status;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
