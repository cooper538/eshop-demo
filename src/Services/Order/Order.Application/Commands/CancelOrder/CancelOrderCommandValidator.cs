using FluentValidation;

namespace EShop.Order.Application.Commands.CancelOrder;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();

        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
