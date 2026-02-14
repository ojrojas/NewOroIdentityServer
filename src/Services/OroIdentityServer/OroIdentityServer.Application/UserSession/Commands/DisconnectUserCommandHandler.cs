// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class DisconnectUserCommandHandler(
    IUserSessionRepository userSessionRepository) : ICommandHandler<DisconnectUserCommand>
{
    public async Task HandleAsync(DisconnectUserCommand request, CancellationToken cancellationToken)
    {
        var sessions = await userSessionRepository.GetActiveUserSessionsByUserIdAsync(request.UserId, cancellationToken);
        foreach (var session in sessions)
        {
            session.Deactivate();
            await userSessionRepository.UpdateUserSessionAsync(session, cancellationToken);
        }
    }
}