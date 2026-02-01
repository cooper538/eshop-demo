using EShop.Order.Application.Commands.CreateOrder;
using FluentValidation.TestHelper;

namespace EShop.Order.UnitTests.Application.Validators;

public class CreateOrderItemDtoValidatorTests
{
    private readonly CreateOrderItemDtoValidator _validator = new();

    // command, shouldPass, failingProperty
    public static TheoryData<CreateOrderItemDto, bool, string?> ItemTestCases =>
        new()
        {
            // Valid
            { CreateItem(), true, null },
            { CreateItem(unitPrice: 0.00m), true, null }, // zero price allowed (free items)
            // ProductId
            { CreateItem(productId: Guid.Empty), false, nameof(CreateOrderItemDto.ProductId) },
            // ProductName
            { CreateItem(productName: ""), false, nameof(CreateOrderItemDto.ProductName) },
            {
                CreateItem(productName: new string('a', 201)),
                false,
                nameof(CreateOrderItemDto.ProductName)
            },
            // Quantity
            { CreateItem(quantity: 0), false, nameof(CreateOrderItemDto.Quantity) },
            { CreateItem(quantity: -1), false, nameof(CreateOrderItemDto.Quantity) },
            // UnitPrice
            { CreateItem(unitPrice: -10.00m), false, nameof(CreateOrderItemDto.UnitPrice) },
        };

    [Theory]
    [MemberData(nameof(ItemTestCases))]
    public void Validate_Item(CreateOrderItemDto item, bool shouldPass, string? failingProperty)
    {
        var result = _validator.TestValidate(item);

        if (shouldPass)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            result.Errors.Should().Contain(e => e.PropertyName == failingProperty);
        }
    }

    private static CreateOrderItemDto CreateItem(
        Guid? productId = null,
        string productName = "Test Product",
        int quantity = 1,
        decimal unitPrice = 10.00m
    )
    {
        return new CreateOrderItemDto(
            productId ?? Guid.NewGuid(),
            productName,
            quantity,
            unitPrice
        );
    }
}
