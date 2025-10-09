using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.Domain.Exceptions;

namespace RoomBooking.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            InvalidBookingException => (HttpStatusCode.BadRequest, nameof(InvalidBookingException)),
            RoomNotFoundException => (HttpStatusCode.NotFound, nameof(RoomNotFoundException)),
            BookingNotFoundException => (HttpStatusCode.NotFound, nameof(BookingNotFoundException)),
            BookingConflictException => (HttpStatusCode.Conflict, nameof(BookingConflictException)),
            _ => (HttpStatusCode.InternalServerError, "ServerError")
        };

        var pd = new ProblemDetails
        {
            Title = title,
            Detail = ex.Message,
            Status = (int)status
        };

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";
        var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(pd, opts);
    }
}
