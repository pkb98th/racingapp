# RacingApp

A multi-stakeholder racing management app, built first for greyhound racing but generic enough for other forms of racing.

Blazor WebAssembly client + ASP.NET Core Web API with Entity Framework Core, secured with ASP.NET Core Identity and JWT bearer tokens, with role-based views and data entry per stakeholder.

## Solution layout

```
RacingApp.sln
src/
  RacingApp.Shared/      Entities, DTOs, enums, role constants (referenced by both other projects)
  RacingApp.Api/         ASP.NET Core Web API: EF Core DbContext, Identity, JWT auth, controllers
  RacingApp.Client/      Blazor WASM SPA: login, role-based views, data entry forms
```

## Stakeholders and permissions

| Role | Views | Data entry |
|---|---|---|
| **TrackAdmin** | Everything | Racetracks, stables, meetings, races, entries, results; provisions owner/trainer/vet records |
| **Owner** | Schedule, results, own racers (with injury history) | Register racers, report injuries |
| **Trainer** | Schedule, own racers | Register racers, add handlers, add/scratch entries, report injuries |
| **Vet** | All injuries/treatments, racer profiles | Record injuries, treatments (with medication + withdrawal periods), clear injuries |
| **Racegoer** | Schedule, races, entries, results, racer profiles | None (read-only) |

Public self-registration creates a **Racegoer** account only. Owner/Trainer/Vet/TrackAdmin logins are provisioned by the track and linked to their person record via `AppUserId` (this powers the "My Racers" view).

## Domain model

- **Racetrack** → hosts **RaceMeetings** (the schedule) → each meeting has **Races**
- **RaceEntry** links a **Racer** to a Race with trap number, weight, handler, and (once run) finish position and time
- **Racer** (the dog) belongs to a **Stable** (kennel), is trained by a **Trainer**, and can have multiple owners via **RacerOwnership** shares
- **Vet** records **Injuries** against a racer and **Treatments** against an injury (medication, dosage, withdrawal period, follow-up)
- **Handler** works for a Trainer and can be assigned to an entry

## Seeded demo accounts (password `Passw0rd!`)

| Email | Role |
|---|---|
| admin@racing.app | TrackAdmin |
| owner@racing.app | Owner (Olive Owner) |
| trainer@racing.app | Trainer (Tara Trainer) |
| vet@racing.app | Vet (Vince Vet) |
| fan@racing.app | Racegoer |

Seed data includes two tracks, two stables, owners, trainers, handlers, three racers, a meeting with two races and entries, and one injury with a treatment.

## Running locally

Prerequisites: .NET 8 SDK.

```bash
# terminal 1 - API (http://localhost:5080, Swagger at /swagger)
cd src/RacingApp.Api
dotnet run

# terminal 2 - Blazor WASM client (http://localhost:5173)
cd src/RacingApp.Client
dotnet run
```

The API defaults to **SQLite** (`racingapp.db`) for zero-setup local development. To use SQL Server:

1. Edit `src/RacingApp.Api/appsettings.json`:
   - set `"Database:Provider"` to `"SqlServer"`
   - set the `SqlServer` connection string
2. Create the initial migration (SQL Server is the intended production database):
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate -p src/RacingApp.Api -s src/RacingApp.Api
   ```
   On startup the API applies pending migrations automatically (and falls back to `EnsureCreated` when no migrations exist).

Client API base URL is configured in `src/RacingApp.Client/wwwroot/appsettings.json`.

> Change `Jwt:Key` before any deployment, and move secrets to user-secrets or environment variables.

## Smoke test

With the API running, verify the seeded data, role restrictions, and the full meeting → race → entry → result and injury → treatment flows:

```bash
./scripts/smoke-test.sh
```

It prints PASS/FAIL per check and exits non-zero if anything fails.

## Roadmap ideas

- Per-stakeholder dashboards with notifications (injury alerts, upcoming races)
- Betting/odds display for racegoers, live results
- Kennel management: feeding, exercise logs, weigh-ins
- Withdrawal-period enforcement: block entries while a treatment withdrawal period is active
- Trainer/owner scoping on entry creation (currently role-based only)
- Full audit logging for vet records (regulatory compliance)
- PWA support for on-track mobile use
