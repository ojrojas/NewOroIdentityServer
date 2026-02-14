namespace OroIdentityServer.Application.Queries;

public record GetRolesByUserIdQuery(UserId UserId) : IQuery<GetRolesByUserIdResponse>
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}