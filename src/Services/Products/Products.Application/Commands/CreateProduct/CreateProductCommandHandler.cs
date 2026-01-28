using EShop.SharedKernel.Services;
using MediatR;
using Products.Application.Data;
using Products.Application.Dtos;

namespace Products.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateProductCommandHandler(
        IProductDbContext dbContext,
        IDateTimeProvider dateTimeProvider
    )
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken
    )
    {
        var product = request.ToEntity(_dateTimeProvider.UtcNow);

        _dbContext.Products.Add(product);

        // Stock is created by ProductCreatedEventHandler via domain event
        // Return DTO with the initial stock quantity from the command
        return ProductDto.FromEntity(product, request.StockQuantity);
    }
}
