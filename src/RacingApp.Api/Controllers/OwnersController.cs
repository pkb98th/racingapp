using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/owners")]
[Authorize]
public class OwnersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OwnersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OwnerDto>>> GetAll() =>
        await _db.Owners.OrderBy(o => o.LastName).Select(o => o.ToDto()).ToListAsync();

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<OwnerDto>> Create(CreateOwnerRequest request)
    {
        var owner = new Owner
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone
        };
        _db.Owners.Add(owner);
        await _db.SaveChangesAsync();
        return Ok(owner.ToDto());
    }
}
