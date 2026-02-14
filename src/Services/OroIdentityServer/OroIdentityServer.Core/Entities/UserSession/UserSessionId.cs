// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public sealed class UserSessionId : BaseValueObject
{
    public Guid Value { get; private set; }

    public UserSessionId(Guid value)
    {
        Value = value;
    }

    public static UserSessionId New() => new(Guid.CreateVersion7());

    protected override IEnumerable<object?> GetEquatibilityComponents()
    {
        yield return Value;
    }
}