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
[Authorize]
public class TreatmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TreatmentsController(AppDbContext db) => _db = db;

    [HttpGet("injury/{injuryId:int}")]
    public async Task<ActionResult<IEnumerable<TreatmentDto>>> GetByInjury(int injuryId)
    {
        if (User.IsInRole(AppRoles.Racegoer) && !User.IsInRole(AppRoles.TrackAdmin))
            return Forbid();

        var treatments = await _db.Treatments
            .Include(t => t.AdministeredBy)
            .Where(t => t.InjuryId == injuryId)
            .OrderBy(t => t.DateAdministered)
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

        var person = await CurrentUser.PersonAsync(_db, User);

        var treatment = new Treatment
        {
            InjuryId = request.InjuryId,
            DateAdministered = request.DateAdministered,
            Description = request.Description,
            Medication = request.Medication,
            Dosage = request.Dosage,
            WithdrawalPeriodDays = request.WithdrawalPeriodDays,
            FollowUpDate = request.FollowUpDate,
            AdministeredByPersonId = person?.Id
        };

        _db.Treatments.Add(treatment);
        injury.Status = InjuryStatus.InTreatment;
        await _db.SaveChangesAsync();

        treatment.AdministeredBy = person;
        return CreatedAtAction(nameof(GetByInjury), new { injuryId = treatment.InjuryId }, treatment.ToDto());
    }
}
