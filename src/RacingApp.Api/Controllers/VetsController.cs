using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/vets")]
[Authorize]
public class VetsController : ControllerBase
{
    private readonly AppDbContext _db;

    public VetsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VetDto>>> GetAll() =>
        await _db.Vets.OrderBy(v => v.LastName).Select(v => v.ToDto()).ToListAsync();

    [HttpPost]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<ActionResult<VetDto>> Create(CreateVetRequest request)
    {
        var vet = new Vet
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            LicenseNumber = request.LicenseNumber,
            ClinicName = request.ClinicName,
            Phone = request.Phone
        };
        _db.Vets.Add(vet);
        await _db.SaveChangesAsync();
        return Ok(vet.ToDto());
    }
}
