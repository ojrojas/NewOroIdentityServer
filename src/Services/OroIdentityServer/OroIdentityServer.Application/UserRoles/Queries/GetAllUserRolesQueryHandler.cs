namespace OroIdentityServer.Application.Queries;

public class GetAllUserRolesQueryHandler(
    ILogger<GetAllUserRolesQueryHandler> logger,
    IUserRolesRepository repository)
    : IQueryHandler<GetAllUserRolesQuery, GetAllUserRolesResponse>
{
    public async Task<GetAllUserRolesResponse> HandleAsync(GetAllUserRolesQuery query, CancellationToken cancellationToken)
    {
        var response = new GetAllUserRolesResponse();
        logger.LogInformation("Handling GetAllUserRolesQuery");
        response.Data = await repository.GetAllRolesAsync(cancellationToken);
        logger.LogInformation("Handled GetAllUserRolesQuery successfully");
        return response;
    }
}