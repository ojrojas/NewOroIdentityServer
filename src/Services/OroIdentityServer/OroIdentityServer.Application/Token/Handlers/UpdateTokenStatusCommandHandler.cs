namespace OroIdentityServer.Application.Token.Handlers;

public class UpdateTokenStatusCommandHandler(ILogger<UpdateTokenStatusCommandHandler> logger, IRepository<Core.Models.Token> repository) : ICommandHandler<UpdateTokenStatusCommand>
{
    public async Task HandleAsync(UpdateTokenStatusCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling UpdateTokenStatusCommand for reference {ref}", command.ReferenceId);
        var token = (await repository.FindAsync(t => t.ReferenceId == command.ReferenceId, cancellationToken)).FirstOrDefault();
        if (token == null) throw new InvalidOperationException("Token not found");
        token.Status = command.NewStatus;
        if (command.RedemptionDate.HasValue) token.RedemptionDate = command.RedemptionDate.Value;
        await repository.UpdateAsync(token, cancellationToken);
    }
}