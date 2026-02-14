namespace OroIdentityServer.Application.Authorization.Queries;

public record GetAuthorizationByIdQuery(Guid Id) : IQuery<GetAuthorizationByIdResponse>
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}