namespace OroIdentityServer.Application.Queries;

public record GetLoginHistoriesByUserIdQuery(UserId UserId) : IQuery<GetLoginHistoriesByUserIdResponse>
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}