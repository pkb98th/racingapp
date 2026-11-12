using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Models;
using RacingApp.Shared;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Data;

public static class DbSeeder
{
    private const string SeedPassword = "Passw0rd!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var db = services.GetRequiredService<AppDbContext>();

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var admin = await EnsureUserAsync(userManager, "admin@racing.app", "Sam Trackadmin", AppRoles.TrackAdmin);
        var ownerUser = await EnsureUserAsync(userManager, "owner@racing.app", "Olive Owner", AppRoles.Owner);
        var trainerUser = await EnsureUserAsync(userManager, "trainer@racing.app", "Tara Trainer", AppRoles.Trainer);
        var vetUser = await EnsureUserAsync(userManager, "vet@racing.app", "Vince Vet", AppRoles.Vet);
        var fanUser = await EnsureUserAsync(userManager, "fan@racing.app", "Fiona Fan", AppRoles.Racegoer);

        if (await db.Racetracks.AnyAsync())
            return;

        var track1 = new Racetrack
        {
            Name = "Willowbrook Greyhound Stadium",
            Address = "12 Circuit Road, Willowbrook",
            TrackLengthMeters = 480,
            Surface = "Sand",
            Capacity = 6000
        };
        var track2 = new Racetrack
        {
            Name = "Harbor Flats Raceway",
            Address = "90 Promenade, Harbor Flats",
            TrackLengthMeters = 515,
            Surface = "Sand",
            Capacity = 4200
        };
        db.Racetracks.AddRange(track1, track2);

        var stable1 = new Stable { Name = "Windrush Kennels", Address = "5 Meadow Lane", Capacity = 40 };
        var stable2 = new Stable { Name = "Seaview Kennels", Address = "3 Cliff Parade", Capacity = 25 };
        db.Stables.AddRange(stable1, stable2);
        await db.SaveChangesAsync();

        var trainer = new Trainer
        {
            FirstName = "Tara",
            LastName = "Trainer",
            LicenseNumber = "TR-2024-018",
            Email = "trainer@racing.app",
            Phone = "555-0102",
            StableId = stable1.Id,
            AppUserId = trainerUser.Id
        };
        var trainer2 = new Trainer
        {
            FirstName = "Hank",
            LastName = "Holloway",
            LicenseNumber = "TR-2023-101",
            Email = "hank@windrushkennels.example",
            Phone = "555-0110",
            StableId = stable2.Id
        };
        var owner = new Owner
        {
            FirstName = "Olive",
            LastName = "Owner",
            Email = "owner@racing.app",
            Phone = "555-0101",
            AppUserId = ownerUser.Id
        };
        var owner2 = new Owner
        {
            FirstName = "Desmond",
            LastName = "Doyle",
            Email = "des.doyle@example.com",
            Phone = "555-0109"
        };
        var vet = new Vet
        {
            FirstName = "Vince",
            LastName = "Vet",
            LicenseNumber = "VET-88231",
            ClinicName = "Trackside Veterinary Clinic",
            Phone = "555-0103",
            AppUserId = vetUser.Id
        };
        db.Trainers.AddRange(trainer, trainer2);
        db.Owners.AddRange(owner, owner2);
        db.Vets.Add(vet);
        await db.SaveChangesAsync();

        var handler = new Handler { FirstName = "Molly", LastName = "Marsh", Phone = "555-0104", TrainerId = trainer.Id };
        var handler2 = new Handler { FirstName = "Piotr", LastName = "Nowak", Phone = "555-0105", TrainerId = trainer2.Id };
        db.Handlers.AddRange(handler, handler2);

        var racer1 = new Racer
        {
            Name = "Bolt From Blue",
            KennelNumber = "WR-001",
            DateOfBirth = new DateOnly(2021, 4, 12),
            Sex = RacerSex.Male,
            Breed = "Greyhound",
            Color = "Blue Brindle",
            MicrochipNumber = "985112009101558",
            WeightKg = 31.5m,
            StableId = stable1.Id,
            TrainerId = trainer.Id
        };
        var racer2 = new Racer
        {
            Name = "Sable Whisper",
            KennelNumber = "WR-002",
            DateOfBirth = new DateOnly(2022, 1, 30),
            Sex = RacerSex.Female,
            Breed = "Greyhound",
            Color = "Black",
            MicrochipNumber = "985112009101559",
            WeightKg = 28.9m,
            StableId = stable1.Id,
            TrainerId = trainer.Id
        };
        var racer3 = new Racer
        {
            Name = "Tidal Dash",
            KennelNumber = "SV-001",
            DateOfBirth = new DateOnly(2020, 11, 5),
            Sex = RacerSex.Male,
            Breed = "Greyhound",
            Color = "Fawn",
            MicrochipNumber = "985112009101560",
            WeightKg = 32.2m,
            StableId = stable2.Id,
            TrainerId = trainer2.Id
        };
        db.Racers.AddRange(racer1, racer2, racer3);
        await db.SaveChangesAsync();

        db.RacerOwnerships.AddRange(
            new RacerOwnership { RacerId = racer1.Id, OwnerId = owner.Id, SharePercent = 100m },
            new RacerOwnership { RacerId = racer2.Id, OwnerId = owner.Id, SharePercent = 50m },
            new RacerOwnership { RacerId = racer2.Id, OwnerId = owner2.Id, SharePercent = 50m },
            new RacerOwnership { RacerId = racer3.Id, OwnerId = owner2.Id, SharePercent = 100m });

        var meeting = new RaceMeeting
        {
            RacetrackId = track1.Id,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            GatesOpen = new TimeOnly(17, 30),
            Status = MeetingStatus.Scheduled
        };
        db.RaceMeetings.Add(meeting);
        await db.SaveChangesAsync();

        var race1 = new Race
        {
            RaceMeetingId = meeting.Id,
            RaceNumber = 1,
            DistanceMeters = 480,
            Grade = "A3",
            PrizeMoney = 750m,
            ScheduledTime = new TimeOnly(18, 15)
        };
        var race2 = new Race
        {
            RaceMeetingId = meeting.Id,
            RaceNumber = 2,
            DistanceMeters = 480,
            Grade = "A5",
            PrizeMoney = 500m,
            ScheduledTime = new TimeOnly(18, 36)
        };
        db.Races.AddRange(race1, race2);
        await db.SaveChangesAsync();

        db.RaceEntries.AddRange(
            new RaceEntry { RaceId = race1.Id, RacerId = racer1.Id, TrainerId = trainer.Id, HandlerId = handler.Id, TrapNumber = 1, WeightKg = 31.5m },
            new RaceEntry { RaceId = race1.Id, RacerId = racer3.Id, TrainerId = trainer2.Id, HandlerId = handler2.Id, TrapNumber = 2, WeightKg = 32.2m },
            new RaceEntry { RaceId = race2.Id, RacerId = racer2.Id, TrainerId = trainer.Id, HandlerId = handler.Id, TrapNumber = 3, WeightKg = 28.9m });

        var injury = new Injury
        {
            RacerId = racer3.Id,
            VetId = vet.Id,
            DateReported = DateTime.UtcNow.AddDays(-10),
            Description = "Slight right fore pad abrasion after trial",
            BodyPart = "Right fore paw",
            Severity = InjurySeverity.Minor,
            Status = InjuryStatus.Recovering
        };
        db.Injuries.Add(injury);
        await db.SaveChangesAsync();

        db.Treatments.Add(new Treatment
        {
            InjuryId = injury.Id,
            VetId = vet.Id,
            DateAdministered = DateTime.UtcNow.AddDays(-9),
            Description = "Cleaned wound, applied antiseptic dressing",
            Medication = "Topical antiseptic",
            Dosage = "Once daily",
            WithdrawalPeriodDays = 0,
            FollowUpDate = DateTime.UtcNow.AddDays(4)
        });

        await db.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager, string email, string fullName, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return existing;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName
        };
        var result = await userManager.CreateAsync(user, SeedPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to seed user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
