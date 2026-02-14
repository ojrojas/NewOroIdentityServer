namespace OroIdentityServer.Application.Application.Queries;

public record GetApplicationsByTenantIdResponse : BaseResponse<IEnumerable<Core.Models.Application>>
{
}