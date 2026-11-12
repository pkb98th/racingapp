namespace RacingApp.Shared;

public static class AppRoles
{
    public const string TrackAdmin = "TrackAdmin";
    public const string Owner = "Owner";
    public const string Trainer = "Trainer";
    public const string Vet = "Vet";
    public const string Racegoer = "Racegoer";

    public static readonly string[] All =
    {
        TrackAdmin, Owner, Trainer, Vet, Racegoer
    };
}
