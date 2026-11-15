using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Data;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _http;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor http) : base(options)
        => _http = http;

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
            .HasFilter("[AppUserId] IS NOT NULL AND [EndDate] IS NULL");

        modelBuilder.Entity<RacerAvailability>()
            .HasIndex(a => new { a.RacerId, a.RacetrackId })
            .IsUnique()
            .HasFilter("[EndDate] IS NULL");

        modelBuilder.Entity<TeamMember>()
            .HasIndex(m => new { m.TeamId, m.PersonId })
            .IsUnique();

        modelBuilder.Entity<RecordShare>()
            .HasIndex(s => new { s.EntityType, s.EntityKey });

        modelBuilder.Entity<Team>().HasOne(t => t.Stable).WithMany().OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Team>().HasOne(t => t.Racetrack).WithMany().OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<TeamMember>().HasOne(m => m.Team).WithMany(t => t.Members).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TeamMember>().HasOne(m => m.Person).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RecordShare>().HasOne(s => s.ToTeam).WithMany().OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<RecordShare>().HasOne(s => s.ToPerson).WithMany().OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<RecordShare>().HasOne(s => s.GrantedBy).WithMany().OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Racer>().HasOne(r => r.Stable).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Racer>().HasOne(r => r.Trainer).WithMany().OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Racer>().HasOne(r => r.Handler).WithMany().OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<RacerOwnership>().HasOne(o => o.Racer).WithMany(r => r.Ownerships).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RacerOwnership>().HasOne(o => o.Owner).WithMany().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RacerAvailability>().HasOne(a => a.Racer).WithMany().OnDelete(DeleteBehavior.Cascade);
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

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(VersionedEntity).IsAssignableFrom(t.ClrType)))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var body = Expression.Equal(
                Expression.Property(parameter, nameof(VersionedEntity.EndDate)),
                Expression.Constant(null, typeof(DateTime?)));
            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(Expression.Lambda(body, parameter));

            modelBuilder.Entity(entityType.ClrType)
                .HasOne(typeof(Team), nameof(VersionedEntity.OwnerTeam))
                .WithMany()
                .HasForeignKey(nameof(VersionedEntity.OwnerTeamId))
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity(entityType.ClrType)
                .HasIndex(nameof(VersionedEntity.Key))
                .IsUnique()
                .HasFilter("[EndDate] IS NULL");
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampAndVersion();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampAndVersion();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void StampAndVersion()
    {
        var now = DateTime.UtcNow;
        var userId = _http.HttpContext?.User.FindFirstValue("sub");

        foreach (var entry in ChangeTracker.Entries<VersionedEntity>().ToList())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.Key == Guid.Empty)
                    entry.Entity.Key = Guid.NewGuid();
                entry.Entity.StartDate = now;
                entry.Entity.EndDate = null;
                entry.Entity.CreatedByUserId = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                var history = BuildHistoryCopy(entry);
                history.EndDate = now;

                entry.Entity.StartDate = now;
                entry.Entity.EndDate = null;
                entry.Entity.CreatedByUserId = userId;

                Add(history);
            }
        }
    }

    private VersionedEntity BuildHistoryCopy(EntityEntry<VersionedEntity> entry)
    {
        var type = entry.Entity.GetType();
        var clone = (VersionedEntity)Activator.CreateInstance(type)!;

        var pkNames = entry.Metadata.FindPrimaryKey()?.Properties
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal) ?? new HashSet<string>(StringComparer.Ordinal);

        foreach (var name in entry.OriginalValues.PropertyNames)
        {
            if (pkNames.Contains(name))
                continue;

            var clrProp = type.GetProperty(name);
            if (clrProp is null || !clrProp.CanWrite)
                continue;

            clrProp.SetValue(clone, entry.OriginalValues[name]);
        }

        return clone;
    }
}
