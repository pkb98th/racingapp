using Microsoft.AspNetCore.Identity;
using RacingApp.Shared;

namespace RacingApp.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
