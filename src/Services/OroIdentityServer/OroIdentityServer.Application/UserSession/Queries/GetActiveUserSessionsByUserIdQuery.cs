namespace OroIdentityServer.Application.Queries;

public record GetActiveUserSessionsByUserIdQuery(UserId UserId) : IQuery<GetActiveUserSessionsByUserIdResponse>
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}