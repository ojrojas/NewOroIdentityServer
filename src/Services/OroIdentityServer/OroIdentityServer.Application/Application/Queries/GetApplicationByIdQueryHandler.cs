// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Application.Queries;

public class GetApplicationByIdQueryHandler(
    IApplicationRepository applicationRepository) : IQueryHandler<GetApplicationByIdQuery, Core.Models.Application?>
{
    public async Task<Core.Models.Application?> HandleAsync(GetApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        return await applicationRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}