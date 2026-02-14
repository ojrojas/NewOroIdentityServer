namespace OroIdentityServer.Application.Revocation.Handlers;

using OroIdentityServer.Application.Commands;

public class CreateRevocationCommandHandler(ILogger<CreateRevocationCommandHandler> logger, IRepository<RevokedJti> repository) : ICommandHandler<CreateRevocationCommand>
{
    public async Task HandleAsync(CreateRevocationCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateRevocationCommand for jti {jti}", command.Jti);
        var entity = new RevokedJti(Core.Models.RevokedJtiId.New(), command.Jti, command.RevokedAt, command.ExpiresAt);
        await repository.AddAsync(entity, cancellationToken);
    }
}
