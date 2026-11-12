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
[Route("api/treatments")]
[Authorize(Roles = $"{AppRoles.TrackAdmin},{AppRoles.Owner},{AppRoles.Trainer},{AppRoles.Vet}")]
public class TreatmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TreatmentsController(AppDbContext db) => _db = db;

    [HttpGet("injury/{injuryId:int}")]
    public async Task<ActionResult<IEnumerable<TreatmentDto>>> GetByInjury(int injuryId)
    {
        var treatments = await _db.Treatments
            .Include(t => t.Vet)
            .Where(t => t.InjuryId == injuryId)
            .OrderByDescending(t => t.DateAdministered)
            .ToListAsync();
        return treatments.Select(t => t.ToDto()).ToList();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Vet)]
    public async Task<ActionResult<TreatmentDto>> Create(CreateTreatmentRequest request)
    {
        var injury = await _db.Injuries.FindAsync(request.InjuryId);
        if (injury is null)
            return BadRequest(new { message = "Unknown injury." });

        var vet = await _db.Vets.FirstOrDefaultAsync(v => v.AppUserId == UserId());
        if (vet is null)
            return Forbid();

        var treatment = new Treatment
        {
            InjuryId = request.InjuryId,
            VetId = vet.Id,
            DateAdministered = request.DateAdministered,
            Description = request.Description,
            Medication = request.Medication,
            Dosage = request.Dosage,
            WithdrawalPeriodDays = request.WithdrawalPeriodDays,
            FollowUpDate = request.FollowUpDate
        };
        _db.Treatments.Add(treatment);

        if (injury.Status != InjuryStatus.Cleared)
            injury.Status = InjuryStatus.InTreatment;

        await _db.SaveChangesAsync();
        return Ok(treatment.ToDto());
    }

    private string UserId() => User.FindFirstValue("sub") ?? string.Empty;
}
