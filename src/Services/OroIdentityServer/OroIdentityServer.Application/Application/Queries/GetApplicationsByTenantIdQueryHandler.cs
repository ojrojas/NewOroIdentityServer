namespace OroIdentityServer.Application.Application.Queries;

public class GetApplicationsByTenantIdQueryHandler(
    ILogger<GetApplicationsByTenantIdQueryHandler> logger,
    IApplicationRepository repository)
    : IQueryHandler<GetApplicationsByTenantIdQuery, GetApplicationsByTenantIdResponse>
{
    public async Task<GetApplicationsByTenantIdResponse> HandleAsync(GetApplicationsByTenantIdQuery query, CancellationToken cancellationToken)
    {
        var response = new GetApplicationsByTenantIdResponse();
        logger.LogInformation("Handling GetApplicationsByTenantIdQuery for TenantId: {TenantId}", query.TenantId.Value);
        response.Data = await repository.GetByTenantIdAsync(query.TenantId, cancellationToken);
        logger.LogInformation("Handled GetApplicationsByTenantIdQuery successfully");
        return response;
    }
}