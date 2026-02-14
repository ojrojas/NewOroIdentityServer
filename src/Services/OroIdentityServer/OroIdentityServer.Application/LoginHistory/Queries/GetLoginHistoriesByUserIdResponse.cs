namespace OroIdentityServer.Application.Queries;

public record GetLoginHistoriesByUserIdResponse : BaseResponse<IEnumerable<LoginHistory>>
{
}