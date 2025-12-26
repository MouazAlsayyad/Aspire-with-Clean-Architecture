using AspireApp.ApiService.Domain.Permissions;
using AspireApp.ApiService.Presentation.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace AspireApp.ApiService.Presentation.Endpoints;

public static class WeatherEndpoints
{
    public static void MapWeatherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/weather").WithTags("Weather");

        group.MapGet("/forecast", GetWeatherForecast)
            .WithName("GetWeatherForecast")
            .WithSummary("Get weather forecast")
            .Produces<IEnumerable<WeatherForecast>>(StatusCodes.Status200OK)
            .RequireAuthorization($"Permission:{PermissionNames.Weather.Read}"); // Requires Weather.Read permission

        group.MapGet("/forecast/admin", GetWeatherForecastAdmin)
            .WithName("GetWeatherForecastAdmin")
            .WithSummary("Get weather forecast (Admin only)")
            .Produces<IEnumerable<WeatherForecast>>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly"); // Requires Admin role
    }

    [AuthorizeRole("User", "Admin", "Manager")]
    private static IResult GetWeatherForecast()
    {
        string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();

        return Results.Ok(forecast);
    }

    [AuthorizeRole("Admin")]
    private static IResult GetWeatherForecastAdmin()
    {
        string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

        var forecast = Enumerable.Range(1, 10).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();

        return Results.Ok(forecast);
    }

    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}

