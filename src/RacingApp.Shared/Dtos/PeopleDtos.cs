using System.ComponentModel.DataAnnotations;

namespace RacingApp.Shared.Dtos;

public class OwnerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class TrainerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? StableName { get; set; }
}

public class VetDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? ClinicName { get; set; }
    public string? Phone { get; set; }
}

public class HandlerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
}

public class StableDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int Capacity { get; set; }
}

public class CreateOwnerRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class CreateTrainerRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required] public string LicenseNumber { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? StableId { get; set; }
}

public class CreateVetRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required] public string LicenseNumber { get; set; } = string.Empty;
    public string? ClinicName { get; set; }
    public string? Phone { get; set; }
}

public class CreateHandlerRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    [Required] public int TrainerId { get; set; }
}

public class CreateStableRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    [Range(1, 500)] public int Capacity { get; set; } = 20;
}

public class CreateRacetrackRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    [Range(50, 5000)] public int TrackLengthMeters { get; set; } = 400;
    public string? Surface { get; set; }
    [Range(1, 200000)] public int Capacity { get; set; } = 1000;
}
