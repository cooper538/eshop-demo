using FluentValidation;

namespace EShop.Order.Application.Commands.CreateOrder;

public sealed class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
