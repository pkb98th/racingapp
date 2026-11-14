using Microsoft.EntityFrameworkCore;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>()
            .HasIndex(p => p.AppUserId)
            .IsUnique()
            .HasFilter("[AppUserId] IS NOT NULL");

        modelBuilder.Entity<RacerAvailability>()
            .HasIndex(a => new { a.RacerId, a.RacetrackId })
            .IsUnique();

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
    }
}
