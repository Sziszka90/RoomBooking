using System.Linq;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Application.Mapping;
using RoomBooking.Application.Services;
using RoomBooking.Data;
using RoomBooking.Data.Repositories;
using RoomBooking.Data.Repositories.Abstraction;
using RoomBooking.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=./data/roombooking.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IRoomsRepository, RoomsRepository>();
builder.Services.AddScoped<IBookingsRepository, BookingsRepository>();

builder.Services.AddScoped<IRoomsService, RoomsService>();
builder.Services.AddScoped<IBookingsService, BookingsService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        try
        {
            var conn = db.Database.GetDbConnection();
            logger.LogInformation("Using database: {DataSource}", conn.DataSource);
        }
        catch { }

        var migrations = db.Database.GetPendingMigrations().Any();
        if (!db.Database.GetMigrations().Any())
        {
            db.Database.EnsureCreated();
            logger.LogInformation("Database created from model (EnsureCreated).");
        }
        else
        {
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
