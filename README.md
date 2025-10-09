# RoomBooking API

A comprehensive Room Booking Web API built with .NET 9 and Entity Framework Core, designed for managing room reservations and availability.

## 🏗️ Architecture

This project follows Clean Architecture principles with clear separation of concerns:

-   **Controllers Layer** - API endpoints and HTTP concerns
-   **Application Layer** - Business logic, services, and DTOs
-   **Domain Layer** - Core business entities and exceptions
-   **Data Layer** - Entity Framework repositories and database context

## 🚀 Features

### Room Management

-   ✅ Create, read, update rooms
-   ✅ Get all rooms
-   ✅ Get room by ID
-   ✅ Check room availability for specific time periods

### Booking Management

-   ✅ Create new bookings
-   ✅ View booking details
-   ✅ Get bookings for specific rooms
-   ✅ Automatic conflict detection and prevention
-   ✅ Delete bookings

### Additional Features

-   ✅ Swagger/OpenAPI documentation
-   ✅ Global exception handling with ProblemDetails
-   ✅ AutoMapper for DTO mapping
-   ✅ SQLite database with Entity Framework migrations
-   ✅ Comprehensive validation and error handling

## 🛠️ Tech Stack

-   **Framework**: .NET 9.0
-   **Database**: SQLite with Entity Framework Core 8.0.11
-   **API Documentation**: Swagger/OpenAPI with Swashbuckle
-   **Mapping**: AutoMapper 12.0.1
-   **Architecture**: Clean Architecture with Repository Pattern

## 📁 Project Structure

```
RoomBooking/
├── Controllers/              # API Controllers
│   ├── RoomsController.cs
│   └── BookingsController.cs
├── Application/             # Application Layer
│   ├── Dtos/               # Data Transfer Objects
│   │   ├── RoomDtos/
│   │   └── BookingDtos/
│   ├── Services/           # Business Logic Services
│   │   ├── RoomsService.cs
│   │   ├── BookingsService.cs
│   │   └── Abstraction/
│   └── Mapping/            # AutoMapper Profiles
│       └── MappingProfile.cs
├── Domain/                 # Domain Entities
│   ├── Room.cs
│   ├── Booking.cs
│   └── Exceptions/
│       └── DomainExceptions.cs
├── Data/                   # Data Access Layer
│   ├── ApplicationDbContext.cs
│   ├── Migrations/         # EF Core Migrations
│   └── Repositories/       # Repository Pattern
│       ├── RoomsRepository.cs
│       ├── BookingsRepository.cs
│       └── Abstraction/
├── Middlewares/            # Custom Middleware
│   └── GlobalExceptionMiddleware.cs
└── Program.cs              # Application Entry Point
```

## 🚀 Getting Started

### Prerequisites

-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
-   [Entity Framework Core Tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)

### Installation

1. **Clone the repository**

    ```bash
    git clone <repository-url>
    cd RoomBooking
    ```

2. **Install EF Core Tools** (if not already installed)

    ```bash
    dotnet tool install --global dotnet-ef --version 8.0.11
    ```

3. **Restore dependencies**

    ```bash
    cd RoomBooking
    dotnet restore
    ```

4. **Apply database migrations**

    ```bash
    dotnet ef database update
    ```

5. **Build the project**

    ```bash
    dotnet build
    ```

6. **Run the application**
    ```bash
    dotnet run
    ```

The API will be available at:

-   **HTTPS**: `https://localhost:7068`
-   **HTTP**: `http://localhost:5133`
-   **Swagger UI**: `https://localhost:7068/swagger`

## 📚 API Endpoints

### Rooms

| Method | Endpoint                                       | Description                         |
| ------ | ---------------------------------------------- | ----------------------------------- |
| GET    | `/api/rooms`                                   | Get all rooms                       |
| GET    | `/api/rooms/{id}`                              | Get room by ID                      |
| POST   | `/api/rooms`                                   | Create a new room                   |
| GET    | `/api/rooms/available?start={start}&end={end}` | Get available rooms for time period |

### Bookings

| Method | Endpoint                      | Description                 |
| ------ | ----------------------------- | --------------------------- |
| POST   | `/api/bookings`               | Create a new booking        |
| GET    | `/api/bookings/{id}`          | Get booking by ID           |
| GET    | `/api/bookings/room/{roomId}` | Get all bookings for a room |
| DELETE | `/api/bookings/{id}`          | Delete a booking            |

## 🗃️ Database Schema

### Room

-   `Id` (int, Primary Key)
-   `Name` (string, Required, Max 100 chars)
-   `Capacity` (int, Required)

### Booking

-   `Id` (int, Primary Key)
-   `RoomId` (int, Foreign Key)
-   `Start` (DateTimeOffset, Required)
-   `End` (DateTimeOffset, Required)
-   `Booker` (string, Required, Max 200 chars)

## 🧪 Development

### Database Migrations

Create a new migration:

```bash
dotnet ef migrations add MigrationName --output-dir Data/Migrations
```

Apply migrations:

```bash
dotnet ef database update
```

Remove last migration:

```bash
dotnet ef migrations remove
```

### Development Tools

The project includes VS Code configuration for:

-   **Debugging**: Press F5 to start debugging
-   **Build Tasks**: Ctrl+Shift+P → "Tasks: Run Task"
-   **Auto-build**: Watch mode with `dotnet watch run`

### Environment Configuration

-   **Development**: Uses SQLite database at `./data/roombooking.db`
-   **Connection String**: Configurable in `appsettings.json`
-   **Logging**: Structured logging with different levels per environment

## 🔧 Configuration

### Database Connection

Default connection string in `appsettings.json`:

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=./data/roombooking.db"
    }
}
```

### Startup Configuration

The application automatically:

-   Applies pending migrations on startup
-   Creates database if no migrations exist
-   Registers all services and repositories
-   Configures middleware pipeline

## ⚡ Performance Considerations

-   **Eager Loading**: Uses `Include()` for related data to minimize queries
-   **Client Evaluation**: DateTimeOffset operations handled in memory for SQLite compatibility
-   **Repository Pattern**: Efficient data access with proper separation of concerns
-   **AutoMapper**: Optimized DTO mapping configuration

## 🛡️ Error Handling

-   **Global Exception Middleware**: Catches and formats all exceptions
-   **ProblemDetails**: Standardized error responses
-   **Domain Exceptions**: Custom business logic exceptions
-   **Validation**: Comprehensive input validation with detailed error messages

## 📝 License

This project is part of the ApiTemplate repository and follows the same licensing terms.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

**Built with ❤️ using .NET 9 and Entity Framework Core**
