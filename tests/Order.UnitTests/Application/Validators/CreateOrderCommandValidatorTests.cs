using EShop.Order.Application.Commands.CreateOrder;
using FluentValidation.TestHelper;

namespace EShop.Order.UnitTests.Application.Validators;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    // command, shouldPass, failingProperty
    public static TheoryData<CreateOrderCommand, bool, string?> CommandTestCases =>
        new()
        {
            // Valid
            { CreateCommand(), true, null },
            // CustomerId
            {
                CreateCommand(customerId: Guid.Empty),
                false,
                nameof(CreateOrderCommand.CustomerId)
            },
            // CustomerEmail
            { CreateCommand(email: ""), false, nameof(CreateOrderCommand.CustomerEmail) },
            { CreateCommand(email: "invalid"), false, nameof(CreateOrderCommand.CustomerEmail) },
            { CreateCommand(email: "invalid@"), false, nameof(CreateOrderCommand.CustomerEmail) },
            {
                CreateCommand(email: "@domain.com"),
                false,
                nameof(CreateOrderCommand.CustomerEmail)
            },
            {
                CreateCommand(email: new string('a', 310) + "@example.com"),
                false,
                nameof(CreateOrderCommand.CustomerEmail)
            },
            // Items
            { CreateCommand(items: []), false, nameof(CreateOrderCommand.Items) },
        };

    [Theory]
    [MemberData(nameof(CommandTestCases))]
    public void Validate_Command(
        CreateOrderCommand command,
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

    private static CreateOrderCommand CreateCommand(
        Guid? customerId = null,
        string email = "customer@example.com",
        IReadOnlyList<CreateOrderItemDto>? items = null
    )
    {
        return new CreateOrderCommand(
            customerId ?? Guid.NewGuid(),
            email,
            items ?? [new CreateOrderItemDto(Guid.NewGuid(), "Product", 1, 10.00m)]
        );
    }
}
