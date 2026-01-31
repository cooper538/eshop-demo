using EShop.Grpc.Product;
using FluentValidation;

namespace EShop.Products.API.Grpc.Validators;

public sealed class GetProductsRequestValidator : AbstractValidator<GetProductsRequest>
{
    public GetProductsRequestValidator()
    {
        RuleFor(x => x.ProductIds).NotEmpty().WithMessage("ProductIds must not be empty");

        RuleForEach(x => x.ProductIds)
            .Must(BeValidGuid)
            .WithMessage("Invalid GUID format: '{PropertyValue}'");
    }

    private static bool BeValidGuid(string value) => Guid.TryParse(value, out _);
}
