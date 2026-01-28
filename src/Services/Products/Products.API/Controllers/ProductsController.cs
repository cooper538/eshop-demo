using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Commands.CreateProduct;
using Products.Application.Commands.UpdateProduct;
using Products.Application.Dtos;
using Products.Application.Queries.GetProductById;
using Products.Application.Queries.GetProducts;

#pragma warning disable IDE1006 // ASP.NET Core convention: controller actions don't need Async suffix

namespace Products.API.Controllers;

/// <summary>
/// External API for product catalog operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List products with optional filtering and pagination.
    /// </summary>
    /// <param name="category">Filter by category</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetProductsResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetProductsResult>> GetProducts(
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var query = new GetProductsQuery(category, page, pageSize);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken ct = default)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="command">Product creation data</param>
    /// <returns>Created product with Location header</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Product update data</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID does not match command ID");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
