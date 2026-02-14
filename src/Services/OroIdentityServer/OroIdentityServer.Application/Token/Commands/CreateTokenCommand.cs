namespace OroIdentityServer.Application.Token.Commands;

public record CreateTokenCommand(
    Core.Models.ApplicationId ApplicationId,
    string Type,
    string ReferenceId,
    string? Payload,
    DateTime CreationDate,
    DateTime? ExpirationDate,
    string Status,
    string? Subject,
    string? Properties
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}