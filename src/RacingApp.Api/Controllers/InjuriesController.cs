using System.Security.Claims;
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
[Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.Owner},{AppRoles.Trainer},{AppRoles.Vet}")]
public class InjuriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public InjuriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InjuryDto>>> GetAll([FromQuery] int? racerId)
    {
        var query = _db.Injuries
            .Include(i => i.Racer)
            .Include(i => i.Vet)
            .Include(i => i.Treatments)
            .AsQueryable();

        if (racerId.HasValue)
            query = query.Where(i => i.RacerId == racerId.Value);

        var injuries = await query.OrderByDescending(i => i.DateReported).ToListAsync();
        return injuries.Select(i => i.ToDto()).ToList();
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.Vet},{AppRoles.Trainer}")]
    public async Task<ActionResult<InjuryDto>> Create(CreateInjuryRequest request)
    {
        var racerExists = await _db.Racers.AnyAsync(r => r.Id == request.RacerId);
        if (!racerExists)
            return BadRequest(new { message = "Unknown racer." });

        var vet = await _db.Vets.FirstOrDefaultAsync(v => v.AppUserId == UserId());
        if (User.IsInRole(AppRoles.Vet) && vet is null)
            return Forbid();

        var injury = new Injury
        {
            RacerId = request.RacerId,
            VetId = vet?.Id,
            DateReported = DateTime.UtcNow,
            Description = request.Description,
            BodyPart = request.BodyPart,
            Severity = request.Severity,
            Status = request.Status
        };
        _db.Injuries.Add(injury);
        await _db.SaveChangesAsync();
        return Ok(injury.ToDto());
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = AppRoles.Vet)]
    public async Task<IActionResult> SetStatus(int id, InjuryStatus status)
    {
        var injury = await _db.Injuries.FindAsync(id);
        if (injury is null)
            return NotFound();

        injury.Status = status;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private string UserId() => User.FindFirstValue("sub") ?? string.Empty;
}
