namespace SkyRoute.Tests.API;

using System.Text.Json;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using SkyRoute.API.Middleware;
using SkyRoute.Application.Exceptions;
using Xunit;

public class ErrorHandlingMiddlewareTests
{
    private static async Task<(int StatusCode, string ContentType, JsonElement Body)> InvokeAsync(
        Exception exceptionToThrow)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ErrorHandlingMiddleware(_ => throw exceptionToThrow);
        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var body = string.IsNullOrEmpty(json)
            ? default
            : JsonSerializer.Deserialize<JsonElement>(json);

        return (context.Response.StatusCode, context.Response.ContentType ?? string.Empty, body);
    }

    // ── FlightNotFoundException ───────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_FlightNotFoundException_Returns404()
    {
        var (status, _, _) = await InvokeAsync(new FlightNotFoundException("f1"));

        Assert.Equal(404, status);
    }

    [Fact]
    public async Task InvokeAsync_FlightNotFoundException_BodyContainsFlightId()
    {
        var (_, _, body) = await InvokeAsync(new FlightNotFoundException("f1"));

        Assert.Contains("f1", body.GetProperty("message").GetString());
    }

    // ── BookingValidationException ────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_BookingValidationException_Returns400()
    {
        var ex = new BookingValidationException(
            new[] { new ValidationFailure("Field", "Error msg") });

        var (status, _, _) = await InvokeAsync(ex);

        Assert.Equal(400, status);
    }

    [Fact]
    public async Task InvokeAsync_BookingValidationException_BodyContainsStructuredDetails()
    {
        var ex = new BookingValidationException(new[]
        {
            new ValidationFailure("Passengers[0].DocumentNumber", "Invalid format.")
        });

        var (_, _, body) = await InvokeAsync(ex);

        Assert.Equal("Validation failed.", body.GetProperty("message").GetString());
        var details = body.GetProperty("details");
        Assert.Equal(JsonValueKind.Array, details.ValueKind);
        Assert.Equal("Invalid format.", details[0].GetProperty("message").GetString());
        Assert.Equal("Passengers[0].DocumentNumber", details[0].GetProperty("field").GetString());
    }

    // ── Unknown exception ─────────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_UnknownException_Returns500WithMessageButNoStackTrace()
    {
        var (status, _, body) = await InvokeAsync(new InvalidOperationException("boom"));

        Assert.Equal(500, status);
        Assert.Equal(JsonValueKind.String, body.GetProperty("message").ValueKind);
        Assert.False(body.TryGetProperty("stackTrace", out _), "Response must not leak stack trace.");
    }

    // ── Content-Type ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_AnyHandledException_ContentTypeIsJson()
    {
        var (_, contentType, _) = await InvokeAsync(new FlightNotFoundException("x"));

        Assert.Equal("application/json", contentType);
    }

    // ── OperationCanceledException ────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_OperationCanceledException_WritesNoBodyAndDoesNotReturn500()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ErrorHandlingMiddleware(_ => throw new OperationCanceledException());
        await middleware.InvokeAsync(context);

        Assert.NotEqual(500, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }
}
