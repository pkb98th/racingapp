using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RacingApp.Api.Models;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _auditing;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Person> People => Set<Person>();
    public DbSet<Stable> Stables => Set<Stable>();
    public DbSet<Racer> Racers => Set<Racer>();
    public DbSet<RacerOwnership> RacerOwnerships => Set<RacerOwnership>();
    public DbSet<RacerAvailability> RacerAvailabilities => Set<RacerAvailability>();
    public DbSet<Racetrack> Racetracks => Set<Racetrack>();
    public DbSet<RaceMeeting> RaceMeetings => Set<RaceMeeting>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<RaceEntry> RaceEntries => Set<RaceEntry>();
    public DbSet<Injury> Injuries => Set<Injury>();
    public DbSet<Treatment> Treatments => Set<Treatment>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<RecordShare> RecordShares => Set<RecordShare>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>()
            .HasIndex(p => p.AppUserId)
            .IsUnique()
            .HasFilter("[AppUserId] IS NOT NULL");

        modelBuilder.Entity<RacerAvailability>()
            .HasIndex(a => new { a.RacerId, a.RacetrackId });

        modelBuilder.Entity<Racer>().HasQueryFilter(r => r.RecordStatus == 1 && r.EndDate == null);
        modelBuilder.Entity<RacerOwnership>().HasQueryFilter(r => r.RecordStatus == 1 && r.EndDate == null);
        modelBuilder.Entity<RacerAvailability>().HasQueryFilter(r => r.RecordStatus == 1 && r.EndDate == null);
        modelBuilder.Entity<Racetrack>().HasQueryFilter(r => r.RecordStatus == 1 && r.EndDate == null);
        modelBuilder.Entity<RaceMeeting>().HasQueryFilter(r => r.RecordStatus == 1 && r.EndDate == null);
        modelBuilder.Entity<Race>().HasQueryFilter(r => r.RecordStatus == 1 && r.EndDate == null);
        modelBuilder.Entity<RaceEntry>().HasQueryFilter(r => r.RecordStatus == 1 && r.EndDate == null);
        modelBuilder.Entity<Injury>().HasQueryFilter(r => r.RecordStatus == 1 && r.EndDate == null);
        modelBuilder.Entity<Treatment>().HasQueryFilter(r => r.RecordStatus == 1 && r.EndDate == null);

        modelBuilder.Entity<TeamMember>()
            .HasOne(m => m.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RecordShare>()
            .HasOne(s => s.Team)
            .WithMany()
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Racer>()
            .HasOne(r => r.Stable)
            .WithMany()
            .HasForeignKey(r => r.StableId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Racer>()
            .HasOne(r => r.Trainer)
            .WithMany()
            .HasForeignKey(r => r.TrainerPersonId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Racer>()
            .HasOne(r => r.Handler)
            .WithMany()
            .HasForeignKey(r => r.HandlerPersonId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<RacerOwnership>().HasOne(o => o.Racer).WithMany(r => r.Ownerships).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RacerOwnership>().HasOne(o => o.Owner).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RacerAvailability>()
            .HasOne(a => a.Racer)
            .WithMany(r => r.Availabilities)
            .HasForeignKey(a => a.RacerId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<RacerAvailability>().HasOne(a => a.Racetrack).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RaceMeeting>().HasOne(m => m.Racetrack).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Race>().HasOne(r => r.RaceMeeting).WithMany(m => m.Races).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RaceEntry>().HasOne(e => e.Race).WithMany(r => r.Entries).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RaceEntry>().HasOne(e => e.Racer).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Injury>().HasOne(i => i.Racer).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Injury>().HasOne(i => i.ReportedBy).WithMany().OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Treatment>().HasOne(t => t.Injury).WithMany(i => i.Treatments).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Treatment>().HasOne(t => t.AdministeredBy).WithMany().OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Person>().HasOne(p => p.Stable).WithMany().OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Racer>().Property(r => r.WeightKg).HasPrecision(5, 2);
        modelBuilder.Entity<RacerOwnership>().Property(o => o.SharePercent).HasPrecision(5, 2);
        modelBuilder.Entity<Race>().Property(r => r.PrizeMoney).HasPrecision(12, 2);
        modelBuilder.Entity<RaceEntry>().Property(e => e.WeightKg).HasPrecision(5, 2);
        modelBuilder.Entity<RaceEntry>().Property(e => e.RaceTimeSeconds).HasPrecision(6, 2);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareManagedRecords();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override int SaveChanges() => SaveChanges(true);

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        PrepareManagedRecords();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(true, cancellationToken);

    private void PrepareManagedRecords()
    {
        if (_auditing)
            return;

        _auditing = true;
        try
        {
            var now = DateTime.UtcNow;
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
            var entries = ChangeTracker.Entries<IManagedRecord>().ToList();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.StartDate == default)
                        entry.Entity.StartDate = now;
                    entry.Entity.RecordStatus = 1;
                    entry.Entity.EndDate = null;
                    entry.Entity.RecordUserId ??= userId;
                    entry.Entity.OwnerTeamId ??= FindFirstTeamForUser(userId);
                    continue;
                }

                if (entry.State != EntityState.Modified || entry.Entity.EndDate is not null)
                {
                    if (entry.State != EntityState.Deleted)
                        continue;

                    var deletedCopy = entry.OriginalValues.ToObject();
                    var deletedKey = entry.Metadata.FindPrimaryKey()?.Properties.SingleOrDefault();
                    if (deletedKey is null)
                        continue;

                    var deletedKeyProperty = deletedCopy.GetType().GetProperty(deletedKey.Name);
                    if (deletedKeyProperty is null || !deletedKeyProperty.CanWrite)
                        continue;

                    deletedKeyProperty.SetValue(
                        deletedCopy,
                        deletedKey.ClrType.IsValueType ? Activator.CreateInstance(deletedKey.ClrType) : null);

                    var deletedManaged = (IManagedRecord)deletedCopy;
                    deletedManaged.RecordStatus = 0;
                    deletedManaged.EndDate = now;
                    deletedManaged.RecordUserId = userId;
                    Add(deletedCopy);

                    entry.Entity.RecordStatus = 0;
                    entry.Entity.EndDate = now;
                    entry.Entity.RecordUserId = userId;
                    entry.State = EntityState.Modified;
                    continue;
                }

                var historical = entry.OriginalValues.ToObject();
                var key = entry.Metadata.FindPrimaryKey()?.Properties.SingleOrDefault();
                if (key is null)
                    continue;

                var keyProperty = historical.GetType().GetProperty(key.Name);
                if (keyProperty is null || !keyProperty.CanWrite)
                    continue;

                keyProperty.SetValue(historical, key.ClrType.IsValueType ? Activator.CreateInstance(key.ClrType) : null);

                var managed = (IManagedRecord)historical;
                managed.RecordStatus = 0;
                managed.EndDate = now;
                Add(historical);

                entry.Entity.RecordStatus = 1;
                entry.Entity.StartDate = now;
                entry.Entity.EndDate = null;
                entry.Entity.RecordUserId = userId;
            }
        }
        finally
        {
            _auditing = false;
        }
    }

    private int? FindFirstTeamForUser(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return TeamMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => (int?)m.TeamId)
            .FirstOrDefault();
    }
}
