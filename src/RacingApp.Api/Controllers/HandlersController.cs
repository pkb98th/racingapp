using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/handlers")]
[Authorize]
public class HandlersController : ControllerBase
{
    private readonly AppDbContext _db;

    public HandlersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HandlerDto>>> GetAll()
    {
        var handlers = await _db.Handlers
            .Include(h => h.Trainer)
            .OrderBy(h => h.LastName)
            .ToListAsync();
        return handlers.Select(h => h.ToDto()).ToList();
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.Trainer}")]
    public async Task<ActionResult<HandlerDto>> Create(CreateHandlerRequest request)
    {
        var trainerExists = await _db.Trainers.AnyAsync(t => t.Id == request.TrainerId);
        if (!trainerExists)
            return BadRequest(new { message = "Unknown trainer." });

        var handler = new Handler
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            TrainerId = request.TrainerId
        };
        _db.Handlers.Add(handler);
        await _db.SaveChangesAsync();
        return Ok(handler.ToDto());
    }
}
