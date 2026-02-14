using Microsoft.AspNetCore.Mvc;
using OroCQRS.Core.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static class ConsentEndpoints
{
    public static WebApplication MapConsentEndpoints(this WebApplication app)
    {
        app.MapPost("/connect/consent",
        [IgnoreAntiforgeryToken]

            async ([FromForm] ConsentRequest req, ISender sender, CancellationToken cancellationToken) =>
            {
                if (req == null || req.UserId == Guid.Empty || req.ClientId == Guid.Empty) return Results.BadRequest(new { error = "invalid_request" });

                var userId = new Core.Models.UserId(req.UserId);
                var appId = new Core.Models.ApplicationId(req.ClientId);
                await sender.Send(new Application.Commands.CreateConsentCommand(userId, appId, req.Scopes ?? string.Empty, req.Remember), cancellationToken);
                return Results.Ok();
            })
        .WithName("CreateConsent")
        .Accepts<ConsentRequest>("application/x-www-form-urlencoded");

        app.MapGet("/connect/consent", 
        [IgnoreAntiforgeryToken]
        
        async (HttpRequest httpRequest, ISender sender, CancellationToken cancellationToken) =>
        {
            if (!httpRequest.Query.TryGetValue("userId", out var userVals) || !httpRequest.Query.TryGetValue("clientId", out var clientVals))
                return Results.BadRequest(new { error = "invalid_request" });

            if (!Guid.TryParse(userVals.ToString(), out var userGuid) || !Guid.TryParse(clientVals.ToString(), out var clientGuid))
                return Results.BadRequest(new { error = "invalid_request" });

            var consent = await sender.Send(new Application.Queries.GetConsentByUserAndClientQuery(new Core.Models.UserId(userGuid), new Core.Models.ApplicationId(clientGuid)), cancellationToken);
            if (consent == null) return Results.NotFound();
            return Results.Json(new { userId = consent.UserId.Value, clientId = consent.ApplicationId.Value, scopes = consent.Scopes, remember = consent.Remember, createdAt = consent.CreatedAt });
        }).WithName("GetConsent");

        return app;
    }

    // Request DTOs
    public record ConsentRequest(Guid UserId, Guid ClientId, string? Scopes, bool Remember);
}
