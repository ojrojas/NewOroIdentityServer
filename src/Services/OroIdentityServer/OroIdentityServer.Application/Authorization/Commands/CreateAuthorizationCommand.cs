namespace OroIdentityServer.Application.Commands;

public record CreateAuthorizationCommand(
    Core.Models.ApplicationId ApplicationId,
    DateTime ExpiresAt,
    string Scopes,
    string Status,
    string Type,
    string? Properties = null
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}