# Job Application Tracker

A microservices-based system for tracking job applications, built in **.NET 8** as a
hands-on study of microservices architecture, formal testing, and CI/CD.

> **Status:** Phase 1 — the **Applications Service** is being built as a standalone
> REST API using Clean Architecture. Other services and the API gateway come in later
> phases (see [Roadmap](#roadmap)).

---

## Architecture

The system is a **monorepo**: one repository holding all services under `services/`,
so the whole system can be cloned, built, and (eventually) orchestrated together with a
single `docker-compose.yml` and one CI/CD pipeline. Each service is independently
buildable (its own solution) and owns its own database.

### Clean Architecture (per service)

Source-code dependencies always point **inward**, toward the Domain. Inner layers know
nothing about outer layers.

```
        ┌───────────────────────────────────────────────┐
        │                     API                        │  controllers, Program.cs
        │              (Presentation layer)              │  (composition root), middleware
        └───────────────┬─────────────────┬──────────────┘
                        │ uses             │ wires up (DI only)
                        ▼                  ▼
        ┌──────────────────────┐   ┌──────────────────────┐
        │      Application      │   │    Infrastructure     │  EF Core, Npgsql,
        │  use cases, DTOs,     │◄──┤  repository impls,    │  migrations
        │  port interfaces,     │   │  external I/O         │
        │  validators           │   └──────────┬───────────┘
        └──────────┬───────────┘               │ uses
                   │ uses                       │
                   ▼                            ▼
        ┌───────────────────────────────────────────────┐
        │                    Domain                       │  entities, value objects,
        │      (entities, value objects, invariants)      │  enums — depends on nothing
        └───────────────────────────────────────────────┘
```

- **Domain** — entities, value objects, enums, invariants. No external dependencies.
- **Application** — use cases, DTOs, port interfaces (e.g. repositories), validation.
- **Infrastructure** — EF Core `DbContext`, repository implementations, migrations.
- **API** — controllers, middleware, Swagger, DI composition root.

The dependency-inversion trick: repository *interfaces* live in **Application** and are
*implemented* in **Infrastructure**, so business logic never references EF Core.

---

## Tech stack

| Concern        | Choice                                   |
| -------------- | ---------------------------------------- |
| Runtime        | ASP.NET Core (.NET 8), C#                |
| Persistence    | PostgreSQL (one database per service)    |
| ORM            | Entity Framework Core (Npgsql provider)  |
| Validation     | FluentValidation                         |
| Testing        | xUnit (unit + integration)               |
| Containers     | Docker / Docker Compose  *(later phase)* |
| API Gateway    | YARP  *(later phase)*                     |
| CI/CD          | GitHub Actions  *(later phase)*          |

---

## Project structure

```
job-tracker/
├─ global.json                     # pins the .NET 8 SDK
├─ services/
│  └─ applications/                # Applications Service (Phase 1)
│     ├─ JobTracker.Applications.sln
│     ├─ src/
│     │  ├─ JobTracker.Applications.Domain/
│     │  ├─ JobTracker.Applications.Application/
│     │  ├─ JobTracker.Applications.Infrastructure/
│     │  └─ JobTracker.Applications.Api/
│     └─ tests/
│        ├─ JobTracker.Applications.UnitTests/
│        └─ JobTracker.Applications.IntegrationTests/
```

---

## Getting started

**Prerequisites:** .NET 8 SDK, Docker Desktop (for PostgreSQL), git, and the EF Core CLI:

```
dotnet tool install --global dotnet-ef --version 8.0.10
```

All commands below are run from `services/applications/`.

**1. Start PostgreSQL**

```
docker compose up -d
```

Runs `postgres:16` on `localhost:5432` (database `applications`, user/password `postgres`/`postgres` — local dev only).

**2. Configure the connection string** (local dev value, stored in user-secrets, not committed)

```
dotnet user-secrets set "ConnectionStrings:ApplicationsDb" "Host=localhost;Port=5432;Database=applications;Username=postgres;Password=postgres" --project src/JobTracker.Applications.Api
```

**3. Apply the database migrations**

```
dotnet ef database update --project src/JobTracker.Applications.Infrastructure --startup-project src/JobTracker.Applications.Infrastructure
```

**4. Run the API**

```
dotnet run --project src/JobTracker.Applications.Api
```

Swagger UI is served in Development at `/swagger` (e.g. `http://localhost:5263/swagger`).

---

## API endpoints

Base route: `/api/job-applications`. Enums (`status`, `source`) are exchanged as strings
(e.g. `"Applied"`, `"LinkedIn"`). Errors are returned as RFC 7807 `ProblemDetails`.

| Verb   | Route                        | Success          | Errors    |
| ------ | ---------------------------- | ---------------- | --------- |
| GET    | `/api/job-applications`      | 200 (list)       | —         |
| GET    | `/api/job-applications/{id}` | 200              | 404       |
| POST   | `/api/job-applications`      | 201 + `Location` | 400       |
| PUT    | `/api/job-applications/{id}` | 200 (updated)    | 400, 404  |
| DELETE | `/api/job-applications/{id}` | 204              | 404       |

Interactive docs: **Swagger UI** at `/swagger` (Development environment).

---

## Testing

> _Unit tests (mocked repositories) and integration tests (real PostgreSQL via
> Testcontainers) — added in a dedicated step._

---

## Roadmap

- [ ] **Phase 1 — Applications Service:** standalone REST API, Clean Architecture,
      EF Core + PostgreSQL, validation, proper HTTP status codes, tests.
- [ ] **Phase 2 — Users/Auth Service.**
- [ ] **Phase 3 — API Gateway (YARP) + Docker Compose orchestration.**
- [ ] **Phase 4 — AI Service.**
- [ ] **CI/CD** — GitHub Actions pipeline (build, test, containerize).

---

## Design decisions

Key trade-offs (separate projects to enforce layering, repository abstraction, DTOs at
the boundary, `Guid` keys, `SalaryRange` value object, RFC 7807 error responses) are
recorded here as the project grows.
