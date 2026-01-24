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

        request.ApplyTo(product);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException($"Product {request.Id} was modified by another user.");
        }

        return product.ToDto();
    }
}
