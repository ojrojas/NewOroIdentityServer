namespace OroIdentityServer.Core.Models;

public sealed class ConsentId(Guid value) : BaseValueObject
{
    public Guid Value { get; private set; } = value;

    public static ConsentId New() => new(Guid.CreateVersion7());

    protected override IEnumerable<object?> GetEquatibilityComponents()
    {
        yield return Value;
    }
}
