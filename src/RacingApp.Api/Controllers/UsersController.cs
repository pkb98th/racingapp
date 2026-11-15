using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Data;
using RacingApp.Api.Models;
using RacingApp.Shared;
using RacingApp.Shared.Dtos;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public UsersController(UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        if (User.IsInRole(AppRoles.TrackAdmin))
            return await ListAllAsync();

        if (User.IsInRole(AppRoles.StableAdmin))
        {
            var person = await CurrentUser.PersonAsync(_db, User);
            if (person?.StableId is null)
                return Forbid();

            var stableId = person.StableId.Value;
            var all = await ListAllAsync();
            return all.Where(u => u.StableId == stableId).ToList();
        }

        return Forbid();
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        var isTrackAdmin = User.IsInRole(AppRoles.TrackAdmin);
        var isStableAdmin = User.IsInRole(AppRoles.StableAdmin);

        if (!isTrackAdmin && !isStableAdmin)
            return Forbid();

        if (!AppRoles.All.Contains(request.Role))
            return BadRequest(new { message = "Unknown role." });

        int? stableId = request.StableId;
        if (isStableAdmin)
        {
            if (request.Role != AppRoles.Trainer && request.Role != AppRoles.Handler)
                return Forbid();

            var person = await CurrentUser.PersonAsync(_db, User);
            if (person?.StableId is null)
                return Forbid();
            stableId = person.StableId;
        }
        else if (request.Role == AppRoles.Trainer || request.Role == AppRoles.Handler)
        {
            if (stableId is null)
                return BadRequest(new { message = "Trainers and handlers must be linked to a stable." });
        }

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Conflict(new { message = "An account with this email already exists." });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            FullName = $"{request.FirstName} {request.LastName}".Trim()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });

        await _userManager.AddToRoleAsync(user, request.Role);

        var access = await AccessContext.LoadAsync(_db, User);
        var ownerTeamId = await AccessContext.TeamForStableAsync(_db, stableId) is { } stableTeam
            ? stableTeam.Id
            : await access.ResolveOwnerTeamIdAsync(_db, null);
        if (ownerTeamId is null)
            return BadRequest(new { message = "No team available to own this record. Create a team first." });

        _db.People.Add(new Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            StableId = stableId,
            AppUserId = user.Id,
            OwnerTeamId = ownerTeamId
        });
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), await ToDtoAsync(user));
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = AppRoles.TrackAdmin)]
    public async Task<IActionResult> SetRole(string id, SetRoleRequest request)
    {
        if (!AppRoles.All.Contains(request.Role))
            return BadRequest(new { message = "Unknown role." });

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains(AppRoles.TrackAdmin)
            && request.Role != AppRoles.TrackAdmin
            && user.Id == CurrentUser.Id(User))
            return BadRequest(new { message = "You cannot remove your own admin role." });

        await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(new[] { request.Role }));
        if (!currentRoles.Contains(request.Role))
            await _userManager.AddToRoleAsync(user, request.Role);

        return NoContent();
    }

    private async Task<List<UserDto>> ListAllAsync()
    {
        var list = new List<UserDto>();
        foreach (var user in await _userManager.Users.OrderBy(u => u.Email).ToListAsync())
            list.Add(await ToDtoAsync(user));
        return list;
    }

    private async Task<UserDto> ToDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var person = await _db.People.Include(p => p.Stable).FirstOrDefaultAsync(p => p.AppUserId == user.Id);
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.DisplayName(),
            Roles = roles.ToList(),
            PersonId = person?.Id,
            StableId = person?.StableId,
            StableName = person?.Stable?.Name
        };
    }
}
