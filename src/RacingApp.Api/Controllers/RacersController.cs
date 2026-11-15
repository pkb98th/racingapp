using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/racers")]
[Authorize]
public class RacersController : ControllerBase
{
    private readonly AppDbContext _db;

    public RacersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RacerDto>>> GetAll([FromQuery] string? search)
    {
        var access = await AccessContext.LoadAsync(_db, User);

        var query = _db.Racers
            .Include(r => r.Stable)
            .Include(r => r.Trainer)
            .Include(r => r.Handler)
            .Include(r => r.OwnerTeam)
            .AsQueryable();

        query = access.FilterReadable(query, _db);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Name.Contains(search) || (r.KennelNumber != null && r.KennelNumber.Contains(search)));

        var racers = await query.OrderBy(r => r.Name).ToListAsync();
        return racers.Select(r => r.ToDto()).ToList();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<RacerDto>>> GetMine()
    {
        var person = await CurrentUser.PersonAsync(_db, User);
        if (person is null)
            return Ok(new List<RacerDto>());

        IQueryable<Racer> query = _db.Racers
            .Include(r => r.Stable)
            .Include(r => r.Trainer)
            .Include(r => r.Handler)
            .Include(r => r.OwnerTeam);

        if (User.IsInRole(AppRoles.Owner))
            query = query.Where(r => r.Ownerships.Any(o => o.OwnerPersonId == person.Id));
        else if (person.StableId is not null)
            query = query.Where(r => r.StableId == person.StableId);
        else
            return Ok(new List<RacerDto>());

        var racers = await query.OrderBy(r => r.Name).ToListAsync();
        return racers.Select(r => r.ToDto()).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RacerDetailDto>> Get(int id)
    {
        var access = await AccessContext.LoadAsync(_db, User);

        var racer = await _db.Racers
            .Include(r => r.Stable)
            .Include(r => r.Trainer)
            .Include(r => r.Handler)
            .Include(r => r.OwnerTeam)
            .Include(r => r.Ownerships).ThenInclude(o => o.Owner)
            .Include(r => r.Availabilities).ThenInclude(a => a.Racetrack)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (racer is null || (!access.ReadsEverything && !access.CanWrite(racer) && !await IsSharedToAsync(access, racer)))
            return NotFound();

        var injuries = await _db.Injuries
            .Include(i => i.ReportedBy)
            .Include(i => i.Treatments).ThenInclude(t => t.AdministeredBy)
            .Where(i => i.RacerId == id)
            .OrderByDescending(i => i.DateReported)
            .ToListAsync();

        var dto = racer.ToDetailDto();
        dto.Injuries = injuries.Select(i => i.ToDto()).ToList();
        return dto;
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.StableAdmin}")]
    public async Task<ActionResult<RacerDto>> Create(CreateRacerRequest request)
    {
        var access = await AccessContext.LoadAsync(_db, User);

        var stableId = await ResolveStableAsync(request.StableId);
        if (stableId is null)
            return Forbid();

        var ownerTeamId = await AccessContext.TeamForStableAsync(_db, stableId) is { } stableTeam
            ? stableTeam.Id
            : await access.ResolveOwnerTeamIdAsync(_db, request.OwnerTeamId);
        if (ownerTeamId is null)
            return BadRequest(new { message = "No team available to own this record. Create a team first." });

        var racer = new Racer
        {
            Name = request.Name,
            KennelNumber = request.KennelNumber,
            DateOfBirth = request.DateOfBirth,
            Sex = request.Sex,
            Breed = request.Breed,
            Color = request.Color,
            MicrochipNumber = request.MicrochipNumber,
            WeightKg = request.WeightKg,
            StableId = stableId.Value,
            TrainerPersonId = request.TrainerPersonId,
            HandlerPersonId = request.HandlerPersonId,
            OwnerTeamId = ownerTeamId
        };

        foreach (var share in request.Owners)
            racer.Ownerships.Add(new RacerOwnership
            {
                OwnerPersonId = share.OwnerPersonId,
                SharePercent = share.SharePercent,
                OwnerTeamId = ownerTeamId
            });

        _db.Racers.Add(racer);
        await _db.SaveChangesAsync();

        racer = await LoadRacerAsync(racer.Id);
        return CreatedAtAction(nameof(Get), new { id = racer!.Id }, racer.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.StableAdmin}")]
    public async Task<IActionResult> Update(int id, UpdateRacerRequest request)
    {
        var access = await AccessContext.LoadAsync(_db, User);

        var racer = await _db.Racers.Include(r => r.Ownerships).FirstOrDefaultAsync(r => r.Id == id);
        if (racer is null)
            return NotFound();

        if (!await access.CanWriteAsync(_db, racer))
            return Forbid();

        if (User.IsInRole(AppRoles.StableAdmin) && !User.IsInRole(AppRoles.TrackAdmin))
        {
            var person = await CurrentUser.PersonAsync(_db, User);
            if (person?.StableId is null || racer.StableId != person.StableId)
                return Forbid();
        }

        racer.Name = request.Name;
        racer.KennelNumber = request.KennelNumber;
        racer.DateOfBirth = request.DateOfBirth;
        racer.Sex = request.Sex;
        racer.Breed = request.Breed;
        racer.Color = request.Color;
        racer.MicrochipNumber = request.MicrochipNumber;
        racer.WeightKg = request.WeightKg;
        racer.IsRetired = request.IsRetired;
        racer.StableId = request.StableId;
        racer.TrainerPersonId = request.TrainerPersonId;
        racer.HandlerPersonId = request.HandlerPersonId;

        if (racer.TrainerPersonId.HasValue)
        {
            var trainerOk = await _db.People.AnyAsync(p => p.Id == racer.TrainerPersonId && p.StableId == racer.StableId);
            if (!trainerOk)
                return BadRequest(new { message = "Trainer must belong to the racer's stable." });
        }

        if (racer.HandlerPersonId.HasValue)
        {
            var handlerOk = await _db.People.AnyAsync(p => p.Id == racer.HandlerPersonId && p.StableId == racer.StableId);
            if (!handlerOk)
                return BadRequest(new { message = "Handler must belong to the racer's stable." });
        }

        _db.RacerOwnerships.RemoveRange(racer.Ownerships);
        foreach (var share in request.Owners)
            racer.Ownerships.Add(new RacerOwnership
            {
                RacerId = racer.Id,
                OwnerPersonId = share.OwnerPersonId,
                SharePercent = share.SharePercent,
                OwnerTeamId = racer.OwnerTeamId
            });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> IsSharedToAsync(AccessContext access, Racer racer)
    {
        if (access.PersonId is null)
            return false;

        var entityType = nameof(Racer);
        var teamIds = access.TeamIds;

        return await _db.RecordShares.AnyAsync(s =>
            s.EntityType == entityType
            && s.EntityKey == racer.Key
            && ((s.ToTeamId != null && teamIds.Contains(s.ToTeamId.Value)) || s.ToPersonId == access.PersonId));
    }

    private async Task<int?> ResolveStableAsync(int? requestedStableId)
    {
        if (User.IsInRole(AppRoles.TrackAdmin))
            return requestedStableId;

        var person = await CurrentUser.PersonAsync(_db, User);
        return person?.StableId;
    }

    private async Task<Racer?> LoadRacerAsync(int id) =>
        await _db.Racers
            .Include(r => r.Stable)
            .Include(r => r.Trainer)
            .Include(r => r.Handler)
            .Include(r => r.OwnerTeam)
            .FirstOrDefaultAsync(r => r.Id == id);
}
