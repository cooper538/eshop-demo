using EShop.SharedKernel.Domain;

namespace EShop.Common.UnitTests.SharedKernel.TestDoubles;

/// <summary>
/// Concrete implementation of ValueObject for testing purposes.
/// </summary>
internal sealed class TestValueObject : ValueObject
{
    public string Value1 { get; }
    public int Value2 { get; }

    public TestValueObject(string value1, int value2)
    {
        Value1 = value1;
        Value2 = value2;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value1;
        yield return Value2;
    }
}
