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
    public async Task<ActionResult<IEnumerable<StableDto>>> GetAll() =>
        await _db.Stables.OrderBy(s => s.Name).Select(s => s.ToDto()).ToListAsync();

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<StableDto>> Create(CreateStableRequest request)
    {
        var stable = new Stable
        {
            Name = request.Name,
            Address = request.Address,
            Capacity = request.Capacity
        };
        _db.Stables.Add(stable);
        await _db.SaveChangesAsync();
        return Ok(stable.ToDto());
    }
}
