using EShop.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.Data;
using Products.Application.Dtos;
using Products.Domain.Entities;

namespace Products.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductDbContext _dbContext;

    public UpdateProductCommandHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDto> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken
    )
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(
            p => p.Id == request.Id,
            cancellationToken
        );

        if (product is null)
        {
            throw NotFoundException.For<ProductEntity>(request.Id);
        }

        // Load stock for response only (read-only, not modified here)
        var stock = await _dbContext
            .Stocks.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProductId == request.Id, cancellationToken);

        if (stock is null)
        {
            throw new NotFoundException($"Stock for product '{request.Id}' was not found.");
        }

        // Only modify the Product aggregate - Stock is updated via domain event
        request.ApplyToProduct(product);

        return ProductDto.FromEntity(product, stock);
    }
}
