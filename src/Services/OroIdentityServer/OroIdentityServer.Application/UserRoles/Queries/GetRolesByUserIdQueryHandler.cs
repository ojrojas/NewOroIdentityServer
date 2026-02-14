namespace OroIdentityServer.Application.Queries;

public class GetRolesByUserIdQueryHandler(
    ILogger<GetRolesByUserIdQueryHandler> logger,
    IUserRolesRepository repository)
    : IQueryHandler<GetRolesByUserIdQuery, GetRolesByUserIdResponse>
{
    public async Task<GetRolesByUserIdResponse> HandleAsync(GetRolesByUserIdQuery query, CancellationToken cancellationToken)
    {
        var response = new GetRolesByUserIdResponse();
        logger.LogInformation("Handling GetRolesByUserIdQuery for UserId: {UserId}", query.UserId.Value);
        response.Data = await repository.GetRolesByUserIdAsync(query.UserId, cancellationToken);
        logger.LogInformation("Handled GetRolesByUserIdQuery successfully");
        return response;
    }
}