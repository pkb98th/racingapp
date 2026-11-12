using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RacingApp.Api.Models;
using RacingApp.Shared.Entities;

namespace RacingApp.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Racetrack> Racetracks => Set<Racetrack>();
    public DbSet<RaceMeeting> RaceMeetings => Set<RaceMeeting>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<RaceEntry> RaceEntries => Set<RaceEntry>();
    public DbSet<Racer> Racers => Set<Racer>();
    public DbSet<RacerOwnership> RacerOwnerships => Set<RacerOwnership>();
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Handler> Handlers => Set<Handler>();
    public DbSet<Stable> Stables => Set<Stable>();
    public DbSet<Vet> Vets => Set<Vet>();
    public DbSet<Injury> Injuries => Set<Injury>();
    public DbSet<Treatment> Treatments => Set<Treatment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Racer>().HasIndex(r => r.MicrochipNumber);
        modelBuilder.Entity<RacerOwnership>().HasIndex(o => new { o.RacerId, o.OwnerId });

        modelBuilder.Entity<Racer>().Property(r => r.WeightKg).HasPrecision(6, 2);
        modelBuilder.Entity<Race>().Property(r => r.PrizeMoney).HasPrecision(12, 2);
        modelBuilder.Entity<RaceEntry>().Property(e => e.WeightKg).HasPrecision(6, 2);
        modelBuilder.Entity<RaceEntry>().Property(e => e.RaceTimeSeconds).HasPrecision(6, 2);
        modelBuilder.Entity<RacerOwnership>().Property(o => o.SharePercent).HasPrecision(5, 2);
    }
}
