namespace OroIdentityServer.Application.Commands;

public record DeleteApplicationCommand(Core.Models.ApplicationId Id) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}