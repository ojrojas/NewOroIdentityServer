namespace OroIdentityServer.Application.Application.Queries;

public record GetApplicationsByTenantIdQuery(TenantId TenantId) : IQuery<GetApplicationsByTenantIdResponse>
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}