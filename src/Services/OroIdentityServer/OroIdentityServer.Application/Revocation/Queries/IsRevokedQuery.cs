namespace OroIdentityServer.Application.Queries;

public record IsRevokedQuery(string Jti) : IQuery<bool>
{
	Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
