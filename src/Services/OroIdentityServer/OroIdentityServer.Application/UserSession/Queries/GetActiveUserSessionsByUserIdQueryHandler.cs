namespace OroIdentityServer.Application.Queries;

public class GetActiveUserSessionsByUserIdQueryHandler(
    ILogger<GetActiveUserSessionsByUserIdQueryHandler> logger,
    IUserSessionRepository repository)
    : IQueryHandler<GetActiveUserSessionsByUserIdQuery, GetActiveUserSessionsByUserIdResponse>
{
    public async Task<GetActiveUserSessionsByUserIdResponse> HandleAsync(GetActiveUserSessionsByUserIdQuery query, CancellationToken cancellationToken)
    {
        var response = new GetActiveUserSessionsByUserIdResponse();
        logger.LogInformation("Handling GetActiveUserSessionsByUserIdQuery for UserId: {UserId}", query.UserId.Value);
        response.Data = await repository.GetActiveUserSessionsByUserIdAsync(query.UserId, cancellationToken);
        logger.LogInformation("Handled GetActiveUserSessionsByUserIdQuery successfully");
        return response;
    }
}