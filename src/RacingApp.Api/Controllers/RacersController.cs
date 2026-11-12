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
[Route("api/racers")]
[Authorize]
public class RacersController : ControllerBase
{
    private readonly AppDbContext _db;

    public RacersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RacerDto>>> GetAll([FromQuery] string? search)
    {
        var query = _db.Racers
            .Include(r => r.Stable)
            .Include(r => r.Trainer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Name.Contains(search) || (r.KennelNumber != null && r.KennelNumber.Contains(search)));

        var racers = await query.OrderBy(r => r.Name).ToListAsync();
        return racers.Select(r => r.ToDto()).ToList();
    }

    [HttpGet("mine")]
    [Authorize(Roles = $"{AppRoles.Owner},{AppRoles.Trainer}")]
    public async Task<ActionResult<IEnumerable<RacerDto>>> GetMine()
    {
        var userId = _userManagerUserId();

        var racers = await _db.Racers
            .Include(r => r.Stable)
            .Include(r => r.Trainer)
            .Include(r => r.Ownerships)
            .ThenInclude(o => o.Owner)
            .ToListAsync();

        var mine = racers.Where(r =>
            (r.Trainer != null && r.Trainer.AppUserId == userId) ||
            r.Ownerships.Any(o => o.Owner.AppUserId == userId));

        return mine.Select(r => r.ToDto()).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RacerDetailDto>> Get(int id)
    {
        var racer = await _db.Racers
            .Include(r => r.Stable)
            .Include(r => r.Trainer)
            .Include(r => r.Ownerships)
            .ThenInclude(o => o.Owner)
            .Include(r => r.Injuries)
            .ThenInclude(i => i.Vet)
            .Include(r => r.Injuries)
            .ThenInclude(i => i.Treatments)
            .Include(r => r.Entries)
            .ThenInclude(e => e.Race)
            .Include(r => r.Entries)
            .ThenInclude(e => e.Trainer)
            .Include(r => r.Entries)
            .ThenInclude(e => e.Handler)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (racer is null)
            return NotFound();

        var dto = new RacerDetailDto
        {
            Id = racer.Id,
            Name = racer.Name,
            KennelNumber = racer.KennelNumber,
            DateOfBirth = racer.DateOfBirth,
            Sex = racer.Sex,
            Breed = racer.Breed,
            Color = racer.Color,
            MicrochipNumber = racer.MicrochipNumber,
            WeightKg = racer.WeightKg,
            IsRetired = racer.IsRetired,
            StableName = racer.Stable?.Name,
            TrainerName = racer.Trainer is null ? null : $"{racer.Trainer.FirstName} {racer.Trainer.LastName}",
            Owners = racer.Ownerships.Where(o => o.EndDate == null).Select(o => o.ToDto()).ToList(),
            Injuries = racer.Injuries.OrderByDescending(i => i.DateReported).Select(i => i.ToDto()).ToList(),
            RecentEntries = racer.Entries
                .OrderByDescending(e => e.Race.ScheduledTime)
                .Take(10)
                .Select(e => e.ToDto())
                .ToList()
        };
        return dto;
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.Owner},{AppRoles.Trainer}")]
    public async Task<ActionResult<RacerDto>> Create(CreateRacerRequest request)
    {
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
            StableId = request.StableId,
            TrainerId = request.TrainerId
        };
        _db.Racers.Add(racer);
        await _db.SaveChangesAsync();

        if (request.OwnerId.HasValue)
        {
            _db.RacerOwnerships.Add(new RacerOwnership
            {
                RacerId = racer.Id,
                OwnerId = request.OwnerId.Value,
                SharePercent = 100m
            });
            await _db.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(Get), new { id = racer.Id }, racer.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.Trainer}")]
    public async Task<IActionResult> Update(int id, UpdateRacerRequest request)
    {
        var racer = await _db.Racers.FindAsync(id);
        if (racer is null)
            return NotFound();

        racer.Name = request.Name;
        racer.Breed = request.Breed;
        racer.Color = request.Color;
        racer.WeightKg = request.WeightKg;
        racer.IsRetired = request.IsRetired;
        racer.StableId = request.StableId;
        racer.TrainerId = request.TrainerId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private string _userManagerUserId() => User.FindFirstValue("sub") ?? string.Empty;
}
