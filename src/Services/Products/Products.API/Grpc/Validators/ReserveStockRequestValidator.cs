using EShop.Grpc.Product;
using FluentValidation;

namespace Products.API.Grpc.Validators;

public sealed class ReserveStockRequestValidator : AbstractValidator<ReserveStockRequest>
{
    public ReserveStockRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId must not be empty")
            .Must(BeValidGuid)
            .WithMessage("OrderId has invalid GUID format: '{PropertyValue}'");

        RuleFor(x => x.Items).NotEmpty().WithMessage("Items must not be empty");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .NotEmpty()
                    .WithMessage("ProductId must not be empty")
                    .Must(BeValidGuid)
                    .WithMessage("ProductId has invalid GUID format: '{PropertyValue}'");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Quantity must be greater than 0");
            });
    }

    private static bool BeValidGuid(string value) => Guid.TryParse(value, out _);
}
