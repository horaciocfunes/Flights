namespace SkyRoute.API.Middleware;

using System.Net;
using System.Text.Json;
using SkyRoute.Application.Exceptions;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — not a server error.
        }
        catch (InvalidSearchCriteriaException ex)
        {
            await WriteError(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (FlightNotFoundException ex)
        {
            await WriteError(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BookingValidationException ex)
        {
            var errors = ex.Errors
                .Select(e => new { field = e.PropertyName, message = e.ErrorMessage });
            await WriteError(context, HttpStatusCode.BadRequest, "Validation failed.", errors);
        }
        catch
        {
            await WriteError(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteError(
        HttpContext context, HttpStatusCode status,
        string message, object? details = null)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { message, details }, JsonOptions);
        await context.Response.WriteAsync(body);
    }
}
