---
name: e2e-test
description: Run E2E test scenarios against running services. Use for happy path testing, unhappy flows, debugging, or when user says "otestuj", "proved test", "zkus flow".
allowed-tools: Bash, Read, Glob, Grep, WebFetch, AskUserQuestion
---

# E2E Test Runner

Execute end-to-end test scenarios against running microservices with automatic debugging and reporting.

## Usage

```
/e2e-test                    # Interactive - asks what to test
/e2e-test happy              # Happy path: create order flow
/e2e-test unhappy            # Unhappy path: out of stock, invalid data
/e2e-test cancel             # Cancel order flow
/e2e-test debug              # Just show service status and debug info
/e2e-test trace <corr-id>    # Trace request across services by CorrelationId
/e2e-test <custom scenario>  # Describe what you want to test
```

## Input

- `$ARGUMENTS` - Test scenario to run or empty for interactive mode

## Architecture Reference

### Services

| Service | Purpose | API Base | gRPC |
|---------|---------|----------|------|
| Gateway | Reverse proxy (YARP) | `/api/*` | - |
| Product API | Product catalog & stock | `/api/products` | `ProductService` |
| Order API | Order management | `/api/orders` | - |
| Notification | Email notifications | - (consumer only) | - |
| Analytics | Order tracking | - (consumer only) | - |

### Databases (PostgreSQL)

| Database | Tables | Purpose |
|----------|--------|---------|
| `productdb` | `Product`, `Stock`, `StockReservation`, `OutboxMessage`, `InboxState` | Product catalog & stock |
| `orderdb` | `Order`, `OrderItem`, `OutboxMessage`, `OutboxState`, `InboxState` | Orders + outbox |
| `notificationdb` | `ProcessedMessages` | Inbox pattern (note: plural "Messages") |

### Stock Behavior (IMPORTANT)

**Stock quantity is NOT decreased** when orders are created. Instead:
- A `StockReservation` record is created with `Status=0` (Active)
- When order is cancelled, reservation changes to `Status=1` (Released)
- The `Stock.Quantity` field represents total inventory, not available inventory

### Message Flow

```
Order API → [gRPC] → Product API (ReserveStock)
    ↓
OrderConfirmedEvent → [RabbitMQ] → Notification + Analytics
```

## Process

### Phase 1: Environment Discovery

**ALWAYS start here.** Run `./tools/e2e-test/discover.sh` to get:

1. **Running services** - ports dynamically assigned by Aspire
2. **Database connection** - PostgreSQL container and credentials
3. **Message broker** - RabbitMQ management URL
4. **Health status** - all service health endpoints

If services are not running, inform user:
```
Services not detected. Start with: dotnet run --project src/AppHost
```

### Phase 2: Scenario Planning

Based on `$ARGUMENTS`, plan the test scenario:

#### `happy` - Order Happy Path
1. GET products → pick one with stock > 0
2. POST order → create order (see API Contract below)
3. Verify: Order status = Confirmed
4. Verify: StockReservation created (Status=0)
5. Verify: Notification inbox has record (`ProcessedMessages` table)
6. Check logs for complete flow

**API Contract - Create Order:**
```json
{
  "customerId": "guid",
  "customerEmail": "email@example.com",
  "items": [{
    "productId": "guid",
    "productName": "required!",
    "quantity": 2,
    "unitPrice": 149.99
  }]
}
```

#### `unhappy` - Failure Scenarios
1. **Out of stock**: Order product with quantity > available
2. **Invalid product**: Order non-existent productId
3. **Invalid data**: Missing required fields
4. Verify: Appropriate error responses
5. Verify: No side effects (stock unchanged, no events)

#### `cancel` - Order Cancellation
1. Create order first (happy path)
2. POST cancel order (see API Contract below)
3. Verify: Order status = Cancelled
4. Verify: StockReservation status changed to 1 (Released)
5. Verify: Cancellation notification processed (`OrderCancelledConsumer`)

**API Contract - Cancel Order:**
```bash
POST /api/orders/{orderId}/cancel
Content-Type: application/json

{"reason": "Customer requested cancellation"}  # Body is REQUIRED!
```

#### `debug` - Service Debug Info
1. Show all service ports and URLs
2. Show database connection info
3. Show recent logs (last 20 lines per service)
4. Show message queue status (RabbitMQ)
5. Show gRPC connectivity status
6. Check service discovery configuration

#### `trace <correlation-id>` - Distributed Request Tracing
1. Run `./tools/e2e-test/trace-correlation.sh <correlation-id>`
2. Display chronologically sorted logs from all services
3. Highlight errors and warnings
4. Show service flow visualization

**Options:**
- `--all-logs` - Search all log files, not just latest
- `--json` - Output as JSON for further processing

**Example:**
```bash
/e2e-test trace 228617a4-175a-4384-a8e2-ade916a78c3f
```

**Output shows:**
- Service-colored log entries (gateway=cyan, order=green, product=yellow, etc.)
- Chronological order across all services
- Error highlighting in red, warnings in yellow

### Phase 3: Execution

Execute scenario step by step. **After each step:**

1. Log the action taken
2. Verify expected outcome
3. If failure detected → **STOP and consult user**

Use these helpers:

```bash
# Service discovery
./tools/e2e-test/discover.sh

# API calls (use Gateway port from discovery)
curl -s http://localhost:$GATEWAY_PORT/api/endpoint | jq '.'

# Database queries - get password first, then query
PG_PASS=$(docker exec <container> printenv POSTGRES_PASSWORD)
docker exec -e PGPASSWORD="$PG_PASS" <container> psql -U postgres -d <db> -c '<SQL>'

# Example:
PG_PASS=$(docker exec postgres-4cdf07e3 printenv POSTGRES_PASSWORD)
docker exec -e PGPASSWORD="$PG_PASS" postgres-4cdf07e3 psql -U postgres -d productdb -c 'SELECT * FROM "StockReservation";'

# Log inspection
./tools/e2e-test/logs.sh <service> [lines]

# Trace correlation ID
./tools/e2e-test/trace-correlation.sh <correlation-id>
```

### Finding Service Ports Manually

If `discover.sh` fails to find services, use this:

```bash
# List all dotnet processes with ports
lsof -i -P -n | grep -E "EShop\.(Ord|Pro|Gat)" | grep LISTEN

# Typical output:
# EShop.Ord 45956  ... TCP 127.0.0.1:49814 (LISTEN)  <- Order API HTTP
# EShop.Pro 45955  ... TCP 127.0.0.1:49815 (LISTEN)  <- Product API HTTP
# EShop.Gat 45954  ... TCP 127.0.0.1:49818 (LISTEN)  <- Gateway HTTP
```

### Phase 4: Reporting

Generate structured report:

```
═══════════════════════════════════════════════════════
  E2E TEST REPORT: <scenario name>
═══════════════════════════════════════════════════════

ENVIRONMENT
  Gateway:      http://localhost:XXXXX ✓
  Order API:    http://localhost:XXXXX ✓
  Product API:  http://localhost:XXXXX ✓
  PostgreSQL:   localhost:XXXXX ✓
  RabbitMQ:     localhost:XXXXX ✓

SCENARIO: <description>

STEPS EXECUTED
  [✓] Step 1: Get products
      → Found 10 products, selected "Cable Management Kit" (stock: 100)

  [✓] Step 2: Create order
      → POST /api/orders → 201 Created
      → OrderId: abc-123-def

  [✓] Step 3: Verify order status
      → DB: Order.Status = 1 (Confirmed)

  [✗] Step 4: Verify stock decreased
      → Expected: 98, Actual: 100
      → FAILURE: Stock not reserved

LOGS (relevant entries)
  [Order.API 21:00:10] Creating order for customer X
  [Order.API 21:00:10] ERROR: gRPC call failed - No address resolver

DIAGNOSIS
  Problem: gRPC service discovery not configured
  Location: src/Common/EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs
  Fix: Add .AddServiceDiscovery() to gRPC client registration

RESULT: FAILED (Step 4)
═══════════════════════════════════════════════════════
```

## Error Handling

When an error blocks the scenario:

1. **STOP execution immediately**
2. **Gather diagnostic info:**
   - Recent logs from affected service
   - Database state
   - Error response details
3. **Present to user with options:**

```
⚠️  Test blocked by error at Step X

Error: <description>
Service: <service name>
Log excerpt:
  <relevant log lines>

Options:
1. Attempt to fix the issue (I'll suggest a fix)
2. Skip this step and continue
3. Abort test and show partial report
4. Debug mode - show all diagnostic info
```

Wait for user decision before proceeding.

## Key Validation Points

### Order Creation (Happy Path)

| Check | Query/Command | Expected |
|-------|---------------|----------|
| API Response | `POST /api/orders` | 200, `status: "Confirmed"` |
| Order in DB | `SELECT * FROM "Order" WHERE "Id"='X'` | `Status = 1` (Confirmed) |
| Reservation created | `SELECT * FROM "StockReservation" WHERE "OrderId"='X'` | 1 row, `Status=0` |
| Stock unchanged | `SELECT "Quantity" FROM "Stock"` | Same as before (stock is NOT decreased) |
| Outbox processed | `SELECT * FROM "OutboxMessage"` | 0 pending (processed) |
| Notification inbox | `SELECT * FROM "ProcessedMessages"` | `OrderConfirmedConsumer` row |

### Order Cancellation

| Check | Query/Command | Expected |
|-------|---------------|----------|
| API Response | `POST /api/orders/{id}/cancel` | 200, `status: "Cancelled"` |
| Order in DB | `SELECT * FROM "Order" WHERE "Id"='X'` | `Status = 3` (Cancelled) |
| Reservation released | `SELECT * FROM "StockReservation" WHERE "OrderId"='X'` | `Status=1` (Released) |
| Notification inbox | `SELECT * FROM "ProcessedMessages"` | `OrderCancelledConsumer` row |

### Stock Low Alert

| Check | Query/Command | Expected |
|-------|---------------|----------|
| Triggered when | Reservation makes available < threshold | `StockLowEvent` published |
| Notification inbox | `SELECT * FROM "ProcessedMessages"` | `StockLowConsumer` row |

### Log Patterns to Verify

| Service | Pattern | Meaning |
|---------|---------|---------|
| Order.API | `Creating order for customer` | Command received |
| Order.API | `Publishing OrderConfirmedEvent` | Event dispatched |
| Product.API | `ReserveStock request received` | gRPC call arrived |
| Product.API | `Stock reserved successfully` | Reservation complete |
| Notification | `Consuming OrderConfirmedEvent` | Event received |
| Notification | `Sending email to` | Email triggered |

## RabbitMQ Diagnostics

Use `./tools/e2e-test/rabbitmq.sh` for message broker debugging:

```bash
./tools/e2e-test/rabbitmq.sh status      # Overview
./tools/e2e-test/rabbitmq.sh queues      # Queue status with message counts
./tools/e2e-test/rabbitmq.sh connections # Active service connections
./tools/e2e-test/rabbitmq.sh consumers   # Consumer registrations
./tools/e2e-test/rabbitmq.sh messages    # Pending message analysis
```

### RabbitMQ Validation Points

| Check | What to look for | Issue if... |
|-------|------------------|-------------|
| **Messages Ready** | Should be 0 after processing | > 0 = stuck messages, consumer issue |
| **Messages Unacked** | Should be 0 or low | High = slow consumer or stuck processing |
| **Connections** | Order, Notification, Analytics | Missing = service not connected |
| **Consumers** | At least 2 per event type | 0 = no one listening |
| **Dead Letter** | Should be empty | Messages = repeated failures |

### Expected Queues (after first message)

```
order-confirmed     - OrderConfirmedEvent consumers
order-rejected      - OrderRejectedEvent consumers
order-cancelled     - OrderCancelledEvent consumers
stock-low           - StockLowEvent consumers
```

### Troubleshooting RabbitMQ

| Symptom | Possible Cause | Debug Command |
|---------|---------------|---------------|
| Messages stuck in queue | Consumer crashed | `rabbitmq.sh consumers` |
| No queues created | No consumers started | `rabbitmq.sh connections` |
| Messages in DLQ | Repeated processing failures | Check consumer logs |
| Missing connections | Service startup failed | Check service health |

## gRPC Diagnostics

Use `./tools/e2e-test/grpc.sh` for inter-service communication debugging:

```bash
./tools/e2e-test/grpc.sh status      # Port and connectivity check
./tools/e2e-test/grpc.sh list        # List services (requires grpcurl)
./tools/e2e-test/grpc.sh test        # Test gRPC calls
./tools/e2e-test/grpc.sh discovery   # Service discovery configuration
```

### gRPC Services

| Service | Proto | Methods |
|---------|-------|---------|
| `ProductService` | `product.proto` | `GetProducts`, `ReserveStock`, `ReleaseStock` |

### gRPC Validation Points

| Check | How | Expected |
|-------|-----|----------|
| Product service reachable | `grpc.sh status` | HTTP 200 on health, gRPC port open |
| Service discovery | `grpc.sh discovery` | `AddServiceDiscovery()` configured |
| Proto matches | Compare proto file | Same version client/server |

### Common gRPC Issues

| Error | Cause | Fix |
|-------|-------|-----|
| `No address resolver for https+http` | Service discovery not configured | Add `.AddServiceDiscovery()` to gRPC client |
| `Connection refused` | Product service not running | Check Product.API health |
| `Deadline exceeded` | Service too slow or unreachable | Check network, increase timeout |
| `Unavailable` | Service crashed mid-request | Check Product.API logs |

### Service Discovery Fix

If you see "No address resolver" error, fix in:
`src/Common/EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs`

```csharp
services
    .AddGrpcClient<ProductServiceClient>(o =>
    {
        o.Address = new Uri(options.ProductService.Url);
    })
    .AddServiceDiscovery()  // <-- ADD THIS LINE
    .ConfigureChannel(...)
```

## Cleanup After Testing

After completing tests, offer cleanup options to user:

### Ask User About Cleanup

```
Test completed. Cleanup options:

1. Keep everything running (for further testing)
2. Stop Aspire services (Ctrl+C in AppHost terminal)
3. Reset test data (purge queues, clear orders)
4. Full cleanup (stop services + reset data)

What would you like to do?
```

### Cleanup Commands

Use `./tools/e2e-test/cleanup.sh` for easy cleanup:

```bash
./tools/e2e-test/cleanup.sh status    # See what needs cleaning
./tools/e2e-test/cleanup.sh data      # Clear test orders, reservations
./tools/e2e-test/cleanup.sh queues    # Purge RabbitMQ queues
./tools/e2e-test/cleanup.sh logs      # Remove logs older than 7 days
./tools/e2e-test/cleanup.sh env       # Remove generated .env
./tools/e2e-test/cleanup.sh all       # Full cleanup
```

Or manually:

```bash
# Stop Aspire (if started by this session)
# Note: AppHost runs in foreground, Ctrl+C stops it

# Reset databases (clear test orders, keep products)
./tools/reset-db.sh

# Purge RabbitMQ queues (remove stuck messages)
./tools/e2e-test/rabbitmq.sh purge <queue-name>
```

### Test Data Cleanup SQL

```sql
-- Clear orders (orderdb)
DELETE FROM "OrderItem";
DELETE FROM "Order";
DELETE FROM "OutboxMessage";
DELETE FROM "OutboxState";
DELETE FROM "InboxState";

-- Clear stock reservations (productdb)
DELETE FROM "StockReservation";

-- Note: Stock.Quantity doesn't need reset - it's not modified by orders

-- Clear processed messages (notificationdb) - note plural "Messages"
DELETE FROM "ProcessedMessages";
```

### When to Clean Up

| Scenario | Recommended Cleanup |
|----------|---------------------|
| Single test run | Option 1 (keep running) |
| End of testing session | Option 2 (stop services) |
| Tests created bad data | Option 3 (reset data) |
| Fresh start needed | Option 4 (full cleanup) |

### Important Notes

- **Never auto-cleanup** without asking user
- **Preserve logs** - useful for debugging issues found during tests
- **Database reset** uses `./tools/reset-db.sh` which re-seeds products
- **RabbitMQ queues** auto-recreate when services restart
- **If you started AppHost** - always offer to stop it at the end (use TaskStop for background tasks)

## Files Reference

| File | Purpose |
|------|---------|
| `./tools/e2e-test/discover.sh` | Service discovery (ports, credentials) |
| `./tools/e2e-test/db-query.sh` | Database queries |
| `./tools/e2e-test/logs.sh` | Log inspection |
| `./tools/e2e-test/api.sh` | API call helper |
| `./tools/e2e-test/rabbitmq.sh` | RabbitMQ diagnostics |
| `./tools/e2e-test/grpc.sh` | gRPC diagnostics |
| `./tools/e2e-test/trace-correlation.sh` | Distributed tracing by CorrelationId |
| `./tools/e2e-test/cleanup.sh` | Test cleanup (default: all) |
| `./tools/reset-db.sh` | Database reset script |
| `src/Services/*/logs/*.log` | Service log files |
| `http://localhost:PORT/swagger` | API documentation |

## Technical Notes

### StockReservation Table Schema
- `ReservedAt` - timestamp when reservation was made
- `ExpiresAt` - when reservation expires
- `Status`: 0 = Active, 1 = Released

### Process Names in lsof
Aspire services use truncated names:
- `EShop.Ord` / `Order.API`
- `EShop.Pro` / `Products.`
- `EShop.Gat` / `Gateway.A`

The `discover.sh` script searches for both naming patterns.
