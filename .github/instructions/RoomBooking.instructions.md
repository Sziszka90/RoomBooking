# Instruction to GitHub Copilot (updated for this repository)

This repository is a Room Booking Web API implemented with .NET (net9.0) and EF Core using SQLite. Keep the guidance below in sync with the project layout and conventions so Copilot suggestions and any generated code match the project structure.

Summary (what this project is)

-   Project name: RoomBooking (assembly/root namespace: RoomBooking)
-   Web API using ASP.NET Core (net9.0) with Swagger/OpenAPI documentation
-   Persistence: Entity Framework Core 8.0.11 with SQLite (local file at `./data/roombooking.db` by default)
-   Testing: Comprehensive unit test suite with Moq for mocking repositories
-   Key concepts: Room and Booking domain entities, repository + service layers, DTOs, AutoMapper mappings, comprehensive logging, and a GlobalExceptionMiddleware producing ProblemDetails responses.

Repository layout and important folders

-   Controllers/ — API controllers (RoomsController, BookingsController) with comprehensive logging
-   Domain/ — domain-specific types and domain exceptions (RoomNotFoundException, BookingNotFoundException, RoomDeletionException, etc.)
-   Models/ — EF entities (Room, Booking) - note: keep domain entities under Domain/ folder
-   Application/
    -   Dtos/ — request/response DTOs (RoomDtos, BookingDtos)
    -   Services/ — application services and their interfaces with logging and business logic
    -   Mapping/ — AutoMapper profiles
-   Data/
    -   ApplicationDbContext.cs — EF Core DbContext
    -   Migrations/ — EF Core migrations (stored in Data folder)
    -   Repositories/ — repository interfaces and EF implementations with logging
-   Middlewares/ — GlobalExceptionMiddleware with ProblemDetails support
-   RoomBooking.Testing/ — Unit test project with Moq for repository mocking
-   .vscode/ — VS Code configuration for debugging and tasks
-   Program.cs — composition root (DI, EF registration, middleware, startup migration handling, Swagger configuration)
-   appsettings.json — default connection string: Data Source=./data/roombooking.db

Domain model (high-level)

-   Room: Id, Name (max 100 chars), Capacity, navigation to Bookings
-   Booking: Id, RoomId, Start (DateTimeOffset), End (DateTimeOffset), Booker (max 200 chars), navigation to Room

API endpoints

-   GET /api/rooms — Get all rooms
-   GET /api/rooms/{id} — Get room by ID (throws RoomNotFoundException if not found)
-   POST /api/rooms — Create a new room
-   DELETE /api/rooms/{id} — Delete room (throws RoomDeletionException if room has bookings)
-   GET /api/rooms/available?start={start}&end={end} — Get available rooms for time period
-   GET /api/bookings/{id} — Get booking by ID (throws BookingNotFoundException if not found)
-   POST /api/bookings — Create a new booking with overlap validation
-   GET /api/bookings/room/{roomId} — Get all bookings for a room
-   DELETE /api/bookings/{id} — Cancel a booking

Coding & architecture conventions for contributors

-   Keep domain entities under `Domain/` (Room.cs, Booking.cs) and place DTOs under `Application/Dtos/`.
-   Services accept DTOs (Create/Update DTOs) and return non-nullable DTOs; throw specific domain exceptions instead of returning null.
-   Service methods should have comprehensive logging using ILogger<T> for operations, warnings, and errors.
-   Repositories expose CRUD/query methods implemented with EF Core in `Data/Repositories` with logging.
-   Use `Include()` for eager loading to minimize database queries and handle SQLite DateTimeOffset limitations with client-side evaluation.
-   Use AutoMapper for mapping entities <-> DTOs. Mapping profiles live in `Application/Mapping`. Register AutoMapper in Program.cs.
-   Business validation (time range ordering, overlap checks) belongs in services; throw specific domain exceptions:
    -   RoomNotFoundException, BookingNotFoundException for missing entities
    -   RoomDeletionException for business rule violations
    -   ArgumentException for invalid input data
-   GlobalExceptionMiddleware translates domain exceptions to appropriate HTTP status codes with ProblemDetails responses.
-   Write comprehensive unit tests in RoomBooking.Testing project using Moq to mock repositories.

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
cd RoomBooking/RoomBooking
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
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
cd RoomBooking/RoomBooking
dotnet run
# then open https://localhost:7068/swagger (HTTPS) or http://localhost:5133/swagger (HTTP)
```

Debugging in VS Code

-   Use F5 to start debugging with the configured launch profiles
-   Available debug configurations: HTTP, HTTPS, and Launch Browser (auto-opens Swagger)
-   Build tasks are configured for dotnet build, watch, clean, and restore

Testing and validation

-   Controllers use model binding and data annotations for basic validation
-   Services enforce business rules and throw domain exceptions; GlobalExceptionMiddleware converts these to ProblemDetails responses
-   Comprehensive unit test suite in RoomBooking.Testing project:
    -   Service layer tests with mocked repositories using Moq
    -   Integration tests for complex scenarios
    -   Test helper classes for common test data setup
    -   Run tests with: `dotnet test RoomBooking.Testing/RoomBooking.Testing.csproj`

Developer tips

-   Keep DTO namespaces under `RoomBooking.Application.Dtos` and service interfaces under `RoomBooking.Application.Services` so DI registrations in Program.cs are consistent
-   All services, repositories, and controllers should inject ILogger<T> for comprehensive logging
-   Commit the generated `Data/Migrations/` folder to source control so team members and CI can apply the same schema
-   SQLite DateTimeOffset limitations: Use client-side evaluation (ToListAsync() then LINQ) for DateTimeOffset comparisons
-   Create specific domain exceptions instead of using generic exceptions for better error handling
-   Use `Include()` for loading related data to minimize database round trips
-   Write unit tests for all service methods, mocking repository dependencies
-   If dotnet-ef cannot find the DbContext at design time, add a design-time factory by implementing `IDesignTimeDbContextFactory<ApplicationDbContext>`
-   Avoid calling `EnsureCreated()` in production; use explicit migrations and a deployment-time migration strategy there

Package versions and dependencies

-   .NET 9.0 (net9.0)
-   Entity Framework Core 8.0.11
-   AutoMapper 12.0.1
-   Swashbuckle.AspNetCore 7.0.0 for Swagger
-   xUnit, Moq, AutoFixture for testing

This document should be updated if project layout, DbContext name, or the persistence strategy changes.

# End of instruction
