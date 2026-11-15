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

        var stableTeam1 = new Team { Name = stable1.Name, Type = TeamType.Stable, StableId = stable1.Id };
        var stableTeam2 = new Team { Name = stable2.Name, Type = TeamType.Stable, StableId = stable2.Id };
        var trackTeam1 = new Team { Name = track1.Name, Type = TeamType.Track, RacetrackId = track1.Id };
        var trackTeam2 = new Team { Name = track2.Name, Type = TeamType.Track, RacetrackId = track2.Id };
        db.Teams.AddRange(stableTeam1, stableTeam2, trackTeam1, trackTeam2);
        await db.SaveChangesAsync();

        var admin = await EnsureUserAsync(userManager, db, "admin@racing.app", "Sam", "Trackadmin", AppRoles.TrackAdmin);
        var stableAdmin1 = await EnsureUserAsync(userManager, db, "stable@racing.app", "Steve", "Stableman", AppRoles.StableAdmin, stable1.Id);
        var stableAdmin2 = await EnsureUserAsync(userManager, db, "stable2@racing.app", "Stella", "Kennel", AppRoles.StableAdmin, stable2.Id);
        var ownerUser = await EnsureUserAsync(userManager, db, "owner@racing.app", "Olive", "Owner", AppRoles.Owner);
        var trainerUser = await EnsureUserAsync(userManager, db, "trainer@racing.app", "Tara", "Trainer", AppRoles.Trainer, stable1.Id);
        var handlerUser = await EnsureUserAsync(userManager, db, "handler@racing.app", "Hank", "Handler", AppRoles.Handler, stable1.Id);
        var vetUser = await EnsureUserAsync(userManager, db, "vet@racing.app", "Vince", "Vet", AppRoles.Vet);
        var fanUser = await EnsureUserAsync(userManager, db, "fan@racing.app", "Fiona", "Fan", AppRoles.Racegoer);

        db.TeamMembers.AddRange(
            new TeamMember { TeamId = stableTeam1.Id, UserId = stableAdmin1.Id, Rights = RecordRights.All },
            new TeamMember { TeamId = stableTeam1.Id, UserId = trainerUser.Id, Rights = RecordRights.All },
            new TeamMember { TeamId = stableTeam1.Id, UserId = handlerUser.Id, Rights = RecordRights.All },
            new TeamMember { TeamId = stableTeam2.Id, UserId = stableAdmin2.Id, Rights = RecordRights.All },
            new TeamMember { TeamId = trackTeam1.Id, UserId = admin.Id, Rights = RecordRights.All },
            new TeamMember { TeamId = trackTeam2.Id, UserId = admin.Id, Rights = RecordRights.All },
            new TeamMember { TeamId = stableTeam1.Id, UserId = ownerUser.Id, Rights = RecordRights.Read },
            new TeamMember { TeamId = stableTeam1.Id, UserId = vetUser.Id, Rights = RecordRights.Write },
            new TeamMember { TeamId = stableTeam1.Id, UserId = fanUser.Id, Rights = RecordRights.Read });
        await db.SaveChangesAsync();

        var ownerPerson = await db.People.SingleAsync(p => p.AppUserId == ownerUser.Id);
        var trainerPerson = await db.People.SingleAsync(p => p.AppUserId == trainerUser.Id);
        var handlerPerson = await db.People.SingleAsync(p => p.AppUserId == handlerUser.Id);

        var racer1 = new Racer
        {
            Name = "Breeze On By",
            KennelNumber = "W-01",
            DateOfBirth = new DateOnly(2021, 4, 12),
            Sex = RacerSex.Bitch,
            Breed = "Greyhound",
            Color = "Black",
            MicrochipNumber = "985100001111111",
            WeightKg = 28.6m,
            StableId = stable1.Id,
            TrainerPersonId = trainerPerson.Id,
            HandlerPersonId = handlerPerson.Id
        };
        var racer2 = new Racer
        {
            Name = "Midnight Dash",
            KennelNumber = "W-02",
            DateOfBirth = new DateOnly(2020, 11, 3),
            Sex = RacerSex.Dog,
            Breed = "Greyhound",
            Color = "Brindle",
            MicrochipNumber = "985100002222222",
            WeightKg = 31.2m,
            StableId = stable1.Id,
            TrainerPersonId = trainerPerson.Id
        };
        var racer3 = new Racer
        {
            Name = "Coastal Flyer",
            KennelNumber = "S-01",
            DateOfBirth = new DateOnly(2021, 8, 25),
            Sex = RacerSex.Dog,
            Breed = "Greyhound",
            Color = "Fawn",
            WeightKg = 30.1m,
            StableId = stable2.Id
        };
        db.Racers.AddRange(racer1, racer2, racer3);
        await db.SaveChangesAsync();

        db.RacerOwnerships.AddRange(
            new RacerOwnership { RacerId = racer1.Id, OwnerPersonId = ownerPerson.Id, SharePercent = 100m },
            new RacerOwnership { RacerId = racer2.Id, OwnerPersonId = ownerPerson.Id, SharePercent = 100m });

        db.RacerAvailabilities.AddRange(
            new RacerAvailability { RacerId = racer1.Id, RacetrackId = track1.Id, IsAvailable = true },
            new RacerAvailability { RacerId = racer2.Id, RacetrackId = track1.Id, IsAvailable = true },
            new RacerAvailability { RacerId = racer2.Id, RacetrackId = track2.Id, IsAvailable = true },
            new RacerAvailability { RacerId = racer3.Id, RacetrackId = track2.Id, IsAvailable = true });

        var meeting = new RaceMeeting
        {
            RacetrackId = track1.Id,
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            GatesOpen = new TimeOnly(11, 30),
            Status = MeetingStatus.Scheduled
        };
        db.RaceMeetings.Add(meeting);
        await db.SaveChangesAsync();

        var race1 = new Race
        {
            RaceMeetingId = meeting.Id,
            RaceNumber = 1,
            DistanceMeters = 480,
            Grade = "A5",
            PrizeMoney = 1200m,
            ScheduledTime = new TimeOnly(14, 0),
            Status = RaceStatus.Scheduled
        };
        var race2 = new Race
        {
            RaceMeetingId = meeting.Id,
            RaceNumber = 2,
            DistanceMeters = 480,
            Grade = "A3",
            PrizeMoney = 1800m,
            ScheduledTime = new TimeOnly(14, 30),
            Status = RaceStatus.Scheduled
        };
        db.Races.AddRange(race1, race2);
        await db.SaveChangesAsync();

        db.RaceEntries.AddRange(
            new RaceEntry { RaceId = race1.Id, RacerId = racer1.Id, TrapNumber = 1, WeightKg = 28.6m, Status = EntryStatus.Entered },
            new RaceEntry { RaceId = race1.Id, RacerId = racer2.Id, TrapNumber = 2, WeightKg = 31.2m, Status = EntryStatus.Entered });

        var injury = new Injury
        {
            RacerId = racer2.Id,
            DateReported = DateTime.UtcNow.AddDays(-2),
            Description = "Slight strain in right shoulder after gallop",
            BodyPart = "Right shoulder",
            Severity = InjurySeverity.Minor,
            Status = InjuryStatus.InTreatment,
            ReportedByPersonId = trainerPerson.Id
        };
        db.Injuries.Add(injury);
        await db.SaveChangesAsync();

        db.Treatments.Add(new Treatment
        {
            InjuryId = injury.Id,
            DateAdministered = DateTime.UtcNow.AddDays(-1),
            Description = "Anti-inflammatory course and rest",
            Medication = "Carprofen",
            Dosage = "50mg twice daily",
            WithdrawalPeriodDays = 5,
            FollowUpDate = DateTime.UtcNow.AddDays(4)
        });

        await db.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        string email,
        string firstName,
        string lastName,
        string role,
        int? stableId = null)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return existing;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = $"{firstName} {lastName}"
        };
        await userManager.CreateAsync(user, SeedPassword);
        await userManager.AddToRoleAsync(user, role);

        db.People.Add(new Person
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            StableId = stableId,
            AppUserId = user.Id
        });
        await db.SaveChangesAsync();

        return user;
    }
}
