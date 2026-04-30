using RacBooking.Application.Commands.Clients.CreateClient;
using RacBooking.Application.Common.Interfaces;
using RacBooking.Application.DTOs;
using RacBooking.Application.Interfaces;
using RacBooking.Application.Queries.Appointments.GetAppointmentsForCalendar;
using RacBooking.Application.Queries.Public.GetPublicSalon;
using RacBooking.Infrastructure.Clients;
using RacBooking.Infrastructure.Persistence;
using RacBooking.Infrastructure.Queries.Appointments;
using RacBooking.Infrastructure.Queries.Public;
using RacBooking.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RacBooking.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers EF Core (PostgreSQL), tenant slug resolution, and scheduling. Connection string from configuration or env.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? BuildConnectionStringFromEnv();
        connectionString = EnsureConnectionTimeout(connectionString, 5);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITenantSlugResolver, TenantSlugResolver>();
        services.AddScoped<ICurrentTenantPortalUser, UnauthenticatedTenantPortalUser>();
        services.AddScoped<ICurrentTenantTimeZoneProvider, CurrentTenantTimeZoneProvider>();
        services.AddScoped<ISchedulingService, SchedulingService>();
        services.AddScoped<IRequestHandler<CreateClientCommand, ClientResponse>, CreateClientCommandHandler>();
        services.AddScoped<IRequestHandler<GetPublicSalonQuery, PublicSalonResponse?>, GetPublicSalonQueryHandler>();
        services.AddScoped<
            IRequestHandler<GetAppointmentsForCalendarQuery, IReadOnlyList<AppointmentCalendarItemResponse>>,
            GetAppointmentsForCalendarQueryHandler>();

        return services;
    }

    private static string BuildConnectionStringFromEnv()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "racbooking_dev";
        var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";
        return $"Host={host};Port={port};Database={database};Username={user};Password={password};";
    }

    private static string EnsureConnectionTimeout(string connectionString, int timeoutSeconds)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return connectionString;
        const string key = "Timeout";
        if (connectionString.Contains(key + "=", StringComparison.OrdinalIgnoreCase)) return connectionString;
        var separator = connectionString.TrimEnd().EndsWith(';') ? "" : ";";
        return connectionString + separator + $"{key}={timeoutSeconds}";
    }
}
