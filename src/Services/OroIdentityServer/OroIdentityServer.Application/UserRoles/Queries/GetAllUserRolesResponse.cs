namespace OroIdentityServer.Application.Queries;

public record GetAllUserRolesResponse : BaseResponse<IEnumerable<UserRole>>
{
}