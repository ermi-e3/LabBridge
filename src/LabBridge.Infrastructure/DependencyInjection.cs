using LabBridge.Application.Common.CurrentUser;
using LabBridge.Application.Common.Idempotency;
using LabBridge.Infrastructure.Common;
using LabBridge.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Configuration key: ConnectionStrings:Default</summary>
    public const string ConnectionStringName = "Default";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Connection string 'ConnectionStrings:{ConnectionStringName}' is not configured. "
                    + "Supply it through user-secrets (Development) or the ConnectionStrings__Default environment variable."
            );

        //var commandTimeoutSeconds = int.TryParse(configuration["Persistence:CommandTimeoutSeconds"], out var timeout) && timeout > 0 ? timeout : 30;
        var sensitiveLogging = bool.TryParse(configuration["Persistence:EnableSensitiveDataLogging"], out var s) && s;
        var detailedErrors = bool.TryParse(configuration["Persistence:EnableDetailedErrors"], out var d) && d;

        // Domain methods take 'now' explicitly; application services should pass this clock's time in.
        services.TryAddSingleton(TimeProvider.System);

        // The one and only DbContext: shared persistence context AND sole migration owner. Scoped (AddDbContext default).
        services.AddDbContext<LabBridgeDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(LabBridgeDbContext).Assembly.GetName().Name);
                    //npgsql.CommandTimeout(commandTimeoutSeconds);
                }
            );
            if (sensitiveLogging)
                options.EnableSensitiveDataLogging();
            if (detailedErrors)
                options.EnableDetailedErrors();
        });

        // ASP.NET Core Identity on the single context. No store options are set on purpose: options such as
        // Stores.SchemaVersion, MaxLengthForKeys or ProtectPersonalData change the EF model (and therefore the migration).
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<LabBridgeDbContext>();

        // IIdempotency
        // IIdempotencyService is a scoped service that manages idempotency records in the database. It is registered with the dependency injection container so that it can be injected into other services or controllers that require idempotency functionality.
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        // CurrentUserService is a scoped service that provides information about the currently authenticated user. It is registered with the dependency injection container so that it can be injected into other services or controllers that require access to the current user's information.
        services.AddScoped<ICurrentUserService, CurrentUserService>();





        return services;
    }
}