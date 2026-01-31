using EShop.Order.Application.Commands.CancelOrder;
using EShop.Order.Application.Commands.CreateOrder;
using EShop.Order.Application.Dtos;
using EShop.Order.Application.Queries.GetOrderById;
using EShop.Order.Application.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable IDE1006 // ASP.NET Core convention: controller actions don't need Async suffix

namespace EShop.Order.API.Controllers;

/// <summary>
/// External API for order operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List orders with optional filtering and pagination.
    /// </summary>
    /// <param name="customerId">Filter by customer ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>Paginated list of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetOrdersResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetOrdersResult>> GetOrders(
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var query = new GetOrdersQuery(customerId, page, pageSize);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get order by ID.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken ct = default)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Create a new order.
    /// </summary>
    /// <param name="command">Order creation data</param>
    /// <returns>Created order with Location header</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateOrderResult>> CreateOrder(CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);
    }

    /// <summary>
    /// Cancel an order.
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Cancellation reason</param>
    /// <returns>Cancellation result</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(CancelOrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CancelOrderResult>> CancelOrder(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken ct = default
    )
    {
        var command = new CancelOrderCommand(id, request.Reason);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

/// <summary>
/// Request body for order cancellation.
/// </summary>
public sealed record CancelOrderRequest(string Reason);
