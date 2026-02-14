namespace OroIdentityServer.Application.Queries;

public record GetRolesByUserIdResponse : BaseResponse<IEnumerable<UserRole>>
{
}