// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public sealed class TokenId(Guid value) : BaseValueObject
{
    public Guid Value { get; private set; } = value;

    public static TokenId New() => new(Guid.CreateVersion7());
    protected override IEnumerable<object> GetEquatibilityComponents()
    {
        yield return Value;
    }
}