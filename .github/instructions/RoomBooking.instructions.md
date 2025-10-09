# Instruction to GitHub Copilot (updated for this repository)

This repository is a Room Booking Web API implemented with .NET (net9.0) and EF Core using SQLite. Keep the guidance below in sync with the project layout and conventions so Copilot suggestions and any generated code match the project structure.

Summary (what this project is)

-   Project name: RoomBooking (assembly/root namespace: RoomBooking)
-   Web API using ASP.NET Core (net9.0)
-   Persistence: Entity Framework Core with SQLite (local file at `./data/roombooking.db` by default)
-   Key concepts: Room and Booking domain entities, repository + service layers, DTOs, AutoMapper mappings, and a GlobalExceptionMiddleware producing ProblemDetails responses.

Repository layout and important folders

-   Controllers/ — API controllers (RoomsController, BookingsController)
-   Domain/ — domain-specific types and domain exceptions
-   Models/ — EF entities (Room, Booking)
-   Application/
    -   Dtos/ — request/response DTOs (RoomDtos, BookingDtos)
    -   Services/ — application services and their interfaces
    -   Mapping/ — AutoMapper profiles
-   Data/
    -   ApplicationDbContext.cs — EF Core DbContext
    -   Repositories/ — repository interfaces and EF implementations
-   Middlewares/ — GlobalExceptionMiddleware
-   Program.cs — composition root (DI, EF registration, middleware, startup migration handling)
-   appsettings.json — default connection string: Data Source=./data/roombooking.db

Domain model (high-level)

-   Room: Id, Name, Capacity, navigation to Bookings
-   Booking: Id, RoomId, Start, End, Booker, navigation to Room

Coding & architecture conventions for contributors

-   Keep domain entities (persistent models) under `Models/` and place DTOs under `Application/Dtos/`.
-   Services accept DTOs (Create/Update DTOs) and return DTOs; service implementations construct domain entities and call repositories.
-   Repositories expose simple CRUD/query methods and are implemented with EF Core in `Data/Repositories`.
-   Use AutoMapper for mapping entities <-> DTOs. Mapping profiles live in `Application/Mapping` (or `Mapping/`). Register AutoMapper in Program.cs.
-   Business validation (time range ordering, overlap checks) belongs in services; throw domain exceptions (e.g., InvalidBookingException, BookingConflictException) — middleware will translate them to proper HTTP codes.

Database and migrations (developer workflow)

1. Ensure the project builds locally

    - cd ApiTemplate/ApiTemplate (or to the project folder) and run `dotnet build`.

2. Install or update the EF CLI tool (match EF Core version from the csproj; this repo uses EF Core 8.x)

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
# or update
dotnet tool update --global dotnet-ef --version 8.0.11
```

3. Create an initial migration (from the project root where the .csproj lives)

```bash
cd ApiTemplate/ApiTemplate
dotnet ef migrations add InitialCreate
```

4. Apply the migration to the local SQLite DB

```bash
dotnet ef database update
```

Notes about startup migrations

-   Program.cs already performs a runtime step that either calls `Database.Migrate()` when migrations exist, or falls back to `Database.EnsureCreated()` when no migrations are present. This makes the app usable during development even if migrations haven't been created, but for stable schema evolution prefer using migrations (steps above).
-   If you used `EnsureCreated()` on a DB already created without migrations and later switch to migrations, consider recreating the DB for a clean migration baseline in dev environments.

How to run locally

```bash
cd ApiTemplate/ApiTemplate
dotnet run
# then open https://localhost:5001/swagger/index.html (or the port printed by the run output)
```

Testing and validation

-   Controllers use model binding and data annotations for basic validation. Services enforce business rules and throw domain exceptions; the GlobalExceptionMiddleware converts these to ProblemDetails responses.

Developer tips

-   Keep DTO namespaces under `RoomBooking.Application.Dtos` and service interfaces under `RoomBooking.Application.Services` so DI registrations in Program.cs are consistent.
-   Commit the generated `Migrations/` folder to source control so team members and CI can apply the same schema.
-   If dotnet-ef cannot find the DbContext at design time, add a design-time factory by implementing `IDesignTimeDbContextFactory<ApplicationDbContext>` to provide a connection string for migrations.
-   Avoid calling `EnsureCreated()` in production; use explicit migrations and a deployment-time migration strategy there.

This document should be updated if project layout, DbContext name, or the persistence strategy changes.

# End of instruction
