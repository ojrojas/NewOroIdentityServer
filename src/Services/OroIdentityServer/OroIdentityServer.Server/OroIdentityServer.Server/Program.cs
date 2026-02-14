using Microsoft.EntityFrameworkCore;
using OroBuildingBlocks.Loggers;
using OroBuildingBlocks.ServiceDefaults;
using OroIdentityServer.Core.Extensions;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Infraestructure.Extensions;
using OroIdentityServer.Application.Extensions;

using Serilog;
using OroIdentityServer.Server.Services;
using OroIdentityServer.Server.Components;
using Scalar.AspNetCore;
using OroIdentityServer.Server.Endpoints;
using OroIdentityServer.Server.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
Log.Logger = LoggerPrinter.CreateSerilogLogger("api", "OroIdentityServer", configuration);

builder.Services.AddOpenApi();

builder.Services.AddControllersWithViews();

// Add Razor Pages for consent UI
builder.Services.AddRazorPages();

// Add cookie authentication for interactive consent login
builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", options =>
    {
        options.LoginPath = "/Consent";
        options.Cookie.Name = "oroidsess";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Provide authentication state for server-side prerendering and interactive render mode
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.AddServicesWritersLogger(configuration);
builder.AddServiceDefaults();
builder.AddApplicationExtensions(configuration);
builder.AddInfraestructureExtensions(configuration);
builder.AddCoreExtensions();
builder.Services.AddAppExtensions();

// Token service (JWT + reference token persistence)
// Load signing key PEM from environment or file into configuration for TokenService
var pemEnv = Environment.GetEnvironmentVariable("TOKEN_SIGNING_KEY_PEM");
if (!string.IsNullOrEmpty(pemEnv))
{
    configuration["TokenService:SigningKeyPrivatePem"] = pemEnv;
}
else
{
    var pemPath = configuration["TokenService:SigningKeyPath"] ?? Environment.GetEnvironmentVariable("TOKEN_SIGNING_KEY_PATH");
    if (!string.IsNullOrEmpty(pemPath) && File.Exists(pemPath))
    {
        configuration["TokenService:SigningKeyPrivatePem"] = File.ReadAllText(pemPath);
    }
    else
    {
        var defaultPath = Path.Combine(builder.Environment.ContentRootPath, "signing_key.pem");
        if (File.Exists(defaultPath))
            configuration["TokenService:SigningKeyPrivatePem"] = File.ReadAllText(defaultPath);
    }
}

builder.Services.Configure<TokenServiceOptions>(configuration.GetSection("TokenService"));
// TokenService depends on ISender (application CQRS) so register as scoped
builder.Services.AddScoped<ITokenService, TokenService>();
// Revocation store
// Prefer DB-backed revocation service (falls back to in-memory if CQRS unavailable)
builder.Services.AddScoped<IRevocationService, RevocationServiceDb>();

builder.Services.AddAntiforgery();

// Key rotation hosted service and options
builder.Services.Configure<KeyRotationOptions>(configuration.GetSection("KeyRotation"));
builder.Services.AddHostedService<KeyRotationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();
    app.MapScalarApiReference();
    using var scope = app.Services.CreateScope();
    var service = scope.ServiceProvider;

    var context = service.GetRequiredService<OroIdentityAppContext>();
    var passwordHasher = service.GetRequiredService<IPasswordHasher>();

    ArgumentNullException.ThrowIfNull(context);
    Console.WriteLine("Deleting database...");
    await context.Database.EnsureDeletedAsync();
    Console.WriteLine("Creating database...");
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("Database created successfully.");
    Console.WriteLine($"Database path: {context.Database.GetDbConnection().Database}");
    Console.WriteLine($"Tables: {string.Join(", ", context.Model.GetEntityTypes().Select(t => t.GetTableName()))}");
    var seedDataPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "bin", "Debug", "net10.0", "Data", "seedData.json");
    // Log configured IdentityWeb URL so we can verify what the seeder will register
    Console.WriteLine($"Configured IdentityWeb:Url = {configuration["IdentityWeb:Url"]}");
    Console.WriteLine($"Configured Identity:Url = {configuration["Identity:Url"]}");
    // await DatabaseSeeder.SeedAsync(
    //     context, 
    //     applicationManager, 
    //     seedDataPath, 
    //     passwordHasher,
    //     configuration);

}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OroIdentityServer.Server.Client._Imports).Assembly);

app.MapConnectEndpoints();
app.MapOpenIdEndpoints();
app.MapRazorPages();

app.Run();
