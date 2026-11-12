using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/trainers")]
[Authorize]
public class TrainersController : ControllerBase
{
    private readonly AppDbContext _db;

    public TrainersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainerDto>>> GetAll()
    {
        var trainers = await _db.Trainers
            .Include(t => t.Stable)
            .OrderBy(t => t.LastName)
            .ToListAsync();
        return trainers.Select(t => t.ToDto()).ToList();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<TrainerDto>> Create(CreateTrainerRequest request)
    {
        var trainer = new Trainer
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            LicenseNumber = request.LicenseNumber,
            Email = request.Email,
            Phone = request.Phone,
            StableId = request.StableId
        };
        _db.Trainers.Add(trainer);
        await _db.SaveChangesAsync();
        return Ok(trainer.ToDto());
    }
}
