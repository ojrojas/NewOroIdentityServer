namespace OroIdentityServer.Application.Commands;

public record AddLoginHistoryCommand(
    UserId UserId,
    string IpAddress,
    string? Country,
    DateTime LoginTime
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}