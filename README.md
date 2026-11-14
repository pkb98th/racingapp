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

## Identity model

- **User** (ASP.NET Core Identity: email + hashed password, JWT login) is linked 1:1 to a **Person** (names, contact details, license, stable membership) via `Person.AppUserId`.
- A user holds **one or many roles**; a person can therefore be an owner, trainer, handler, vet, racegoer — or several at once.
- Public self-registration always creates a **Racegoer**. All other accounts are provisioned by an administrator through the Accounts page (`/api/users`).

## Roles and permissions

| Role | Views | Data entry |
|---|---|---|
| **TrackAdmin** | Everything | Racetracks, stables, meetings, races, entries (only racers flagged available), results; provisions all accounts and roles |
| **StableAdmin** | Own stable: racers, staff, availability | Adds racers to the stable; adds trainer/handler accounts; flags racers available per track |
| **Owner** | Schedule, results, own racers (via ownership) | None (read-only) |
| **Trainer** | Schedule, stable racers | Report injuries |
| **Handler** | Schedule, stable racers | None (read-only) |
| **Vet** | All injuries/treatments, racer profiles | Record injuries, treatments (medication, dosage, withdrawal period), clear injuries |
| **Racegoer** | Schedule, races, entries, results, racer profiles | None (read-only) |

## Domain model

- **User** 1:1 **Person**; roles live on the user (multi-role supported)
- **Stable** kennels **Racers**; a racer can switch stables
- **RacerOwnership** links **Persons with the Owner role** to racers with a share percentage; replacing the ownerships transfers the racer to new owners
- **RacerAvailability** is a per-(racer, racetrack) flag set by the stable; **TrackAdmin can only enter racers flagged available for the meeting's track** (enforced server-side)
- **Racetrack** hosts **RaceMeetings** (the schedule) → each meeting has **Races** → **RaceEntry** links a racer with trap number, weight, and (once run) finish position and time
- **Injuries** (reported by vet/trainer/stable admin) and **Treatments** (vet only, with medication, dosage, withdrawal period, follow-up) hang off racers

## Seeded demo accounts (password `Passw0rd!`)

| Email | Role |
|---|---|
| admin@racing.app | TrackAdmin |
| stable@racing.app | StableAdmin (Windrush Kennels) |
| stable2@racing.app | StableAdmin (Seaview Kennels) |
| owner@racing.app | Owner (Olive Owner) |
| trainer@racing.app | Trainer (Tara Trainer, Windrush) |
| handler@racing.app | Handler (Hank Handler, Windrush) |
| vet@racing.app | Vet (Vince Vet) |
| fan@racing.app | Racegoer |

Seed data includes two tracks, two stables, three racers (with ownerships and availability flags), a meeting with two races and entries, and one injury with a treatment.

> **Schema changed?** If you previously ran an older build, delete `src/RacingApp.Api/racingapp.db` before starting — the API recreates and reseeds it.

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
