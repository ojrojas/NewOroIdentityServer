namespace OroIdentityServer.Application.Queries;

public record GetTokenByReferenceQuery(string Reference) : IQuery<Core.Models.Token?>
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}