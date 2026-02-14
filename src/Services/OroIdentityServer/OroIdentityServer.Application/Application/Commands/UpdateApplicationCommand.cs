namespace OroIdentityServer.Application.Commands;

public record UpdateApplicationCommand(Core.Models.Application Application) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}