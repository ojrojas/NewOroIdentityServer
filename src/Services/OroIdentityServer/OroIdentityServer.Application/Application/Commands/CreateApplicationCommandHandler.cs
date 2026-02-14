// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class CreateApplicationCommandHandler(
    IApplicationRepository applicationRepository) : ICommandHandler<CreateApplicationCommand, Core.Models.ApplicationId>
{
    public async Task<Core.Models.ApplicationId> HandleAsync(CreateApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = new Core.Models.Application(
            Core.Models.ApplicationId.New(),
            Core.Models.ClientSecret.New(),
            request.Application.RedirectUris,
            request.Application.GrantTypes, 
            request.Application.Scopes,
            request.Application.TenantId
        );
        
        
        application.Validate();

        await applicationRepository.AddAsync(application, cancellationToken);

        return application.Id;
    }
}