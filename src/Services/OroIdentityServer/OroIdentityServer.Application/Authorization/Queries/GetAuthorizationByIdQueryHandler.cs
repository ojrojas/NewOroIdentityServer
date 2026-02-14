namespace OroIdentityServer.Application.Authorization.Queries;

public class GetAuthorizationByIdQueryHandler(
    ILogger<GetAuthorizationByIdQueryHandler> logger,
    IAuthorizationRepository repository)
    : IQueryHandler<GetAuthorizationByIdQuery, GetAuthorizationByIdResponse>
{
    public async Task<GetAuthorizationByIdResponse> HandleAsync(GetAuthorizationByIdQuery query, CancellationToken cancellationToken)
    {
        var response = new GetAuthorizationByIdResponse();
        logger.LogInformation("Handling GetAuthorizationByIdQuery for Id: {Id}", query.Id);
        response.Data = await repository.GetAuthorizationByAsync(query.Id, cancellationToken);
        logger.LogInformation("Handled GetAuthorizationByIdQuery successfully");
        return response;
    }
}