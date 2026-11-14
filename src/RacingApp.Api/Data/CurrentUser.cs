using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Data;

public static class CurrentUser
{
    public static string? Id(ClaimsPrincipal user) =>
        user.FindFirstValue("sub");

    public static async Task<Person?> PersonAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var userId = Id(user);
        if (userId is null)
            return null;

        return await db.People.FirstOrDefaultAsync(p => p.AppUserId == userId);
    }
}
