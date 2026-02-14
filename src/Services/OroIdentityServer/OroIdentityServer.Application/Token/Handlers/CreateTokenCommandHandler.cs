namespace OroIdentityServer.Application.Handlers;

public class CreateTokenCommandHandler(ILogger<CreateTokenCommandHandler> logger, IRepository<Core.Models.Token> repository) : ICommandHandler<CreateTokenCommand>
{
    public async Task HandleAsync(CreateTokenCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateTokenCommand for reference {ref}", command.ReferenceId);

        var application = await repository.CurrentContext.OfType<Core.Models.Application>()
            .FirstOrDefaultAsync(a => a.Id == command.ApplicationId, cancellationToken)
            ?? throw new InvalidOperationException("Application not found");

        var token = new Core.Models.Token(
            Core.Models.TokenId.New(),
            application,
            null,
            Guid.NewGuid().ToString(),
            command.CreationDate,
            command.ExpirationDate,
            command.Payload,
            command.Properties,
            null,
            command.ReferenceId,
            command.Status,
            command.Subject,
            command.Type
        );

        await repository.AddAsync(token, cancellationToken);
    }
}