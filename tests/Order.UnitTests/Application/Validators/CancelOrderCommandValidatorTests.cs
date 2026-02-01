using EShop.Order.Application.Commands.CancelOrder;
using FluentValidation.TestHelper;

namespace EShop.Order.UnitTests.Application.Validators;

public class CancelOrderCommandValidatorTests
{
    private readonly CancelOrderCommandValidator _validator = new();

    // command, shouldPass, failingProperty
    public static TheoryData<CancelOrderCommand, bool, string?> CommandTestCases =>
        new()
        {
            // Valid
            { new CancelOrderCommand(Guid.NewGuid(), "Customer request"), true, null },
            { new CancelOrderCommand(Guid.NewGuid(), new string('a', 500)), true, null }, // max length
            // OrderId
            {
                new CancelOrderCommand(Guid.Empty, "Reason"),
                false,
                nameof(CancelOrderCommand.OrderId)
            },
            // Reason
            {
                new CancelOrderCommand(Guid.NewGuid(), ""),
                false,
                nameof(CancelOrderCommand.Reason)
            },
            {
                new CancelOrderCommand(Guid.NewGuid(), new string('a', 501)),
                false,
                nameof(CancelOrderCommand.Reason)
            },
        };

    [Theory]
    [MemberData(nameof(CommandTestCases))]
    public void Validate_Command(
        CancelOrderCommand command,
        bool shouldPass,
        string? failingProperty
    )
    {
        var result = _validator.TestValidate(command);

        if (shouldPass)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            result.Errors.Should().Contain(e => e.PropertyName == failingProperty);
        }
    }
}
