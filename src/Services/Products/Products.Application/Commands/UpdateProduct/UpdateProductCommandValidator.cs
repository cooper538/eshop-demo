using FluentValidation;

namespace Products.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);

        RuleFor(x => x.Price).GreaterThan(0);

        RuleFor(x => x.LowStockThreshold).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}
