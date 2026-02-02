using EShop.Order.Application.Commands.CreateOrder;
using FluentValidation.TestHelper;

namespace EShop.Order.UnitTests.Application.Validators;

public class CreateOrderItemDtoValidatorTests
{
    private readonly CreateOrderItemDtoValidator _validator = new();

    public static TheoryData<CreateOrderItemDto, bool, string?> ItemTestCases =>
        new()
        {
            // Valid
            { CreateItem(), true, null },
            // ProductId
            { CreateItem(productId: Guid.Empty), false, nameof(CreateOrderItemDto.ProductId) },
            // Quantity
            { CreateItem(quantity: 0), false, nameof(CreateOrderItemDto.Quantity) },
            { CreateItem(quantity: -1), false, nameof(CreateOrderItemDto.Quantity) },
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

    private static CreateOrderItemDto CreateItem(Guid? productId = null, int quantity = 1)
    {
        return new CreateOrderItemDto(productId ?? Guid.NewGuid(), quantity);
    }
}
