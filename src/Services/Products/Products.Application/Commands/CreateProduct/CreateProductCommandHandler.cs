using MediatR;
using Products.Application.Data;
using Products.Application.Dtos;

namespace Products.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductDbContext _dbContext;

    public CreateProductCommandHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken
    )
    {
        var product = request.ToEntity();

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Stock is created by ProductCreatedEventHandler via domain event
        // Return DTO with the initial stock quantity from the command
        return ProductDto.FromEntity(product, request.StockQuantity);
    }
}
