namespace OroIdentityServer.Application.Queries;

public record GetActiveUserSessionsByUserIdResponse : BaseResponse<IEnumerable<UserSession>>
{
}