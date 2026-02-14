namespace OroIdentityServer.Application.Commands;

public record CreateConsentCommand(
    UserId UserId,
    Core.Models.ApplicationId ApplicationId,
    string Scopes,
    bool Remember
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
