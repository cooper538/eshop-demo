using FluentValidation;

namespace EShop.Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(320);

        RuleFor(x => x.Items).NotEmpty();

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemDtoValidator());
    }
}
