# Task 07: External REST API

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-04, task-05, task-06 |

## Summary
Create OrdersController with all external API endpoints and configure Program.cs.

## Scope

### OrdersController
- [ ] Create `OrdersController` in Order.API/Controllers
  ```csharp
  [ApiController]
  [Route("api/[controller]")]
  public class OrdersController : ControllerBase
  ```

- [ ] `POST /api/orders` - Create order
  - Returns 201 Created with Location header
  - Body: CreateOrderCommand
  ```csharp
  [HttpPost]
  [ProducesResponseType(typeof(CreateOrderResult), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<CreateOrderResult>> CreateOrder(CreateOrderCommand command)
  {
      var result = await _mediator.Send(command);
      return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);
  }
  ```

- [ ] `GET /api/orders/{id}` - Get order by ID
  - Returns 200 OK or 404 Not Found
  ```csharp
  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken ct)
  ```

- [ ] `GET /api/orders` - List orders with filtering
  - Query params: customerId (optional), page, pageSize
  ```csharp
  [HttpGet]
  [ProducesResponseType(typeof(GetOrdersResult), StatusCodes.Status200OK)]
  public async Task<ActionResult<GetOrdersResult>> GetOrders(
      [FromQuery] Guid? customerId,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20,
      CancellationToken ct = default)
  ```

- [ ] `POST /api/orders/{id}/cancel` - Cancel order
  - Returns 200 OK (with success/failure in body) or 404 Not Found
  ```csharp
  [HttpPost("{id:guid}/cancel")]
  [ProducesResponseType(typeof(CancelOrderResult), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<CancelOrderResult>> CancelOrder(
      Guid id,
      [FromBody] CancelOrderRequest request,
      CancellationToken ct)
  {
      var command = new CancelOrderCommand(id, request.Reason);
      var result = await _mediator.Send(command, ct);
      return Ok(result);
  }
  ```

### Program.cs Configuration
- [ ] Configure YAML configuration loading
- [ ] `builder.AddServiceDefaults()` (Aspire)
- [ ] `builder.Services.AddControllers()`
- [ ] `builder.Services.AddOpenApi()` (Swagger)
- [ ] `builder.Services.AddMediatR()` - scan Application assembly
- [ ] `builder.Services.AddCommonBehaviors()` (validation, logging, tracking)
- [ ] `builder.Services.AddValidatorsFromAssemblyContaining<IOrderDbContext>()`
- [ ] `builder.AddInfrastructure()` (DbContext registration)
- [ ] `builder.Services.AddErrorHandling()`
- [ ] `builder.Services.AddCorrelationId()`
- [ ] Middleware pipeline: UseCorrelationId, UseErrorHandling, MapControllers, MapDefaultEndpoints

### AppHost Registration
- [ ] Add PostgreSQL resource "orderdb" in AppHost (if not exists)
- [ ] Add "order-api" project reference
- [ ] Configure service with database reference

## Reference Implementation
See `ProductsController` and `Program.cs` in Products.API

## Related Specs
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 2: HTTP Endpoints)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 5: Implementation)

---
## Notes
(Updated during implementation)
