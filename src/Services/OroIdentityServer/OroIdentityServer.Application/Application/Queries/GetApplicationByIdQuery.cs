// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Application.Queries;

public record GetApplicationByIdQuery(
    Core.Models.ApplicationId Id) : IQuery<Core.Models.Application?>
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}