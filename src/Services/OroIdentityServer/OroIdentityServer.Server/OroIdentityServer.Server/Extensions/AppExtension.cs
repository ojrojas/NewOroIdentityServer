using OroIdentityServer.Server.Services;

namespace OroIdentityServer.Server.Extensions;

public static class AppExtension
{
    public static IServiceCollection AddAppExtensions(this IServiceCollection services)
    {
        // TokenService depends on scoped application services (ISender etc.),
        // so register it as scoped to avoid consuming scoped services from a singleton.
        services.AddScoped<ITokenService, TokenService>();
        // Prefer DB-backed revocation service (scoped). Remove singleton in-memory registration
        // to avoid lifetime conflicts and ambiguous registrations.
        services.AddScoped<IRevocationService, RevocationServiceDb>();

        return services;
    }
}
