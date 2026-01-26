using FluentValidation;
using Products.Application.Dtos;

namespace Products.Application.Commands.ReserveStock;

public sealed class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());
    }
}

public sealed class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
{
    public OrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}
