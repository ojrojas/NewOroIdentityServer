namespace OroIdentityServer.Application.Queries;

public record GetAllUserRolesQuery() : IQuery<GetAllUserRolesResponse>
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}