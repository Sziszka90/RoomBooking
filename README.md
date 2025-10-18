# Room Booking API

A .NET 9.0 Web API for managing room bookings with SQLite database.

## Features

-   **Room Management**: Create, read, update, delete rooms
-   **Booking System**: Book rooms with overlap validation and pricing
-   **User History**: Track booking history with filtering options
-   **Room Swapping**: Transfer bookings between rooms
-   **Price Filtering**: Find available rooms within budget range

## Tech Stack

-   **.NET 9.0** - Web API framework
-   **Entity Framework Core 8.0** - ORM with SQLite
-   **AutoMapper** - Object mapping
-   **Swagger/OpenAPI** - API documentation
-   **xUnit + Moq** - Unit testing

## Quick Start

1. **Clone and run**:

    ```bash
    git clone <repository-url>
    cd RoomBooking
    dotnet run --project RoomBooking
    ```

2. **Access API**:

    - API: `https://localhost:5133/api`
    - Swagger: `https://localhost:5133/swagger/index.html`

3. **Run tests**:
    ```bash
    dotnet test
    ```

## Database

SQLite database (`data/roombooking.db`) is automatically created on first run. Uses DELETE journal mode (no WAL files).

## API Endpoints

### Rooms

-   `GET /api/rooms` - List all rooms
-   `GET /api/rooms/{id}` - Get room by ID
-   `GET /api/rooms/available` - Find available rooms (with date/price filters)
-   `POST /api/rooms` - Create room
-   `DELETE /api/rooms/{id}` - Delete room

### Bookings

-   `GET /api/bookings/{id}` - Get booking by ID
-   `GET /api/bookings/for-room/{roomId}` - Get bookings for room
-   `GET /api/bookings/user-history/{booker}` - Get user booking history
-   `POST /api/bookings` - Create booking
-   `POST /api/bookings/swap` - Swap booking to different room
-   `PUT /api/bookings/cancel/{id}` - Cancel booking
-   `POST /api/bookings/print-user-history/{booker}` - Log user history to console

## Architecture

-   **N Layers** with UnitOfWork pattern
-   **Custom Domain Exceptions** with global error handling
-   **Repository Pattern** for data access
-   **Service Layer** for business logic
-   **AutoMapper** for DTO transformations
