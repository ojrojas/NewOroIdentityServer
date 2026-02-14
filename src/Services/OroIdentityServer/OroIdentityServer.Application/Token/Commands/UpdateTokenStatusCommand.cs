namespace OroIdentityServer.Application.Token.Commands;

public record UpdateTokenStatusCommand(
    string ReferenceId,
    string NewStatus,
    DateTime? RedemptionDate = null
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}