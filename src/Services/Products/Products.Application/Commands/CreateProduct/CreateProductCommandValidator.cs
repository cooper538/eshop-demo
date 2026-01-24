using FluentValidation;

namespace Products.Application.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);

        RuleFor(x => x.Price).GreaterThan(0);

        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);

        RuleFor(x => x.LowStockThreshold).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}
