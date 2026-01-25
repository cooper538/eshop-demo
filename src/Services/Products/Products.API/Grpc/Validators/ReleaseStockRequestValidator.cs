using EShop.Grpc.Product;
using FluentValidation;

namespace Products.API.Grpc.Validators;

public sealed class ReleaseStockRequestValidator : AbstractValidator<ReleaseStockRequest>
{
    public ReleaseStockRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId must not be empty")
            .Must(BeValidGuid)
            .WithMessage("OrderId has invalid GUID format: '{PropertyValue}'");
    }

    private static bool BeValidGuid(string value) => Guid.TryParse(value, out _);
}
