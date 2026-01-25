using FluentValidation;

namespace Order.Application.Commands.CreateOrder;

public sealed class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Quantity).GreaterThan(0);

        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
