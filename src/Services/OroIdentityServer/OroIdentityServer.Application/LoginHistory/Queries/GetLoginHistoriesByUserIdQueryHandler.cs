namespace OroIdentityServer.Application.Queries;

public class GetLoginHistoriesByUserIdQueryHandler(
    ILogger<GetLoginHistoriesByUserIdQueryHandler> logger,
    ILoginHistoryRepository repository)
    : IQueryHandler<GetLoginHistoriesByUserIdQuery, GetLoginHistoriesByUserIdResponse>
{
    public async Task<GetLoginHistoriesByUserIdResponse> HandleAsync(GetLoginHistoriesByUserIdQuery query, CancellationToken cancellationToken)
    {
        var response = new GetLoginHistoriesByUserIdResponse();
        logger.LogInformation("Handling GetLoginHistoriesByUserIdQuery for UserId: {UserId}", query.UserId.Value);
        response.Data = await repository.GetLoginHistoriesByUserIdAsync(query.UserId, cancellationToken);
        logger.LogInformation("Handled GetLoginHistoriesByUserIdQuery successfully");
        return response;
    }
}