namespace OroIdentityServer.Application.Handlers;


public class IsRevokedQueryHandler(ILogger<IsRevokedQueryHandler> logger, IRepository<RevokedJti> repository) : IQueryHandler<IsRevokedQuery, bool>
{
    public async Task<bool> HandleAsync(IsRevokedQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling IsRevokedQuery for jti {jti}", request.Jti);
        var list = await repository.FindAsync(r => r.Jti == request.Jti, cancellationToken);
        var item = list.FirstOrDefault();
        if (item == null) return false;
        if (item.ExpiresAt.HasValue && item.ExpiresAt.Value < DateTime.UtcNow) return false;
        return true;
    }
}
