namespace OroIdentityServer.Application.Handlers;

public class GetTokenByReferenceQueryHandler(
    ILogger<GetTokenByReferenceQueryHandler> logger, 
    IRepository<Core.Models.Token> repository) : IQueryHandler<GetTokenByReferenceQuery, Core.Models.Token?>
{
    public async Task<Core.Models.Token?> HandleAsync(GetTokenByReferenceQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetTokenByReferenceQuery for {ref}", request.Reference);
        return (await repository.FindAsync(t => t.ReferenceId == request.Reference, cancellationToken)).FirstOrDefault();
    }
}