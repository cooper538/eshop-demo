# E2E Happy Flow Validation - Test Specification

## Overview

This document describes test scenarios for validating remaining happy flows that have not yet been verified. Each scenario includes prerequisites, test steps, and expected outcomes.

**Already verified flows:**
- ✅ Create Order (happy path) - stock reserved, order confirmed, notification sent
- ✅ Cancel Order - order cancelled, stock released, notification sent

---

## 1. Order Rejection Flow

### Description
When a customer attempts to order more items than available in stock, the order should be rejected with appropriate notification.

### Prerequisites
- All services running via `dotnet run --project src/AppHost`
- Product with limited stock (e.g., quantity = 5)

### Test Steps

```bash
# 1. Create a product with low stock
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Limited Item",
    "description": "Product with limited stock for testing",
    "price": 99.99,
    "stockQuantity": 5,
    "lowStockThreshold": 2,
    "category": "test"
  }'
# Save the returned productId

# 2. Create order exceeding available stock
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerEmail": "test@example.com",
    "items": [
      {
        "productId": "<PRODUCT_ID>",
        "quantity": 10
      }
    ]
  }'
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 200 OK (not 201) |
| Response status | `"status": "Rejected"` |
| Response reason | Contains "Insufficient stock" |
| Order in DB | Status = Rejected |
| Stock unchanged | Product stockQuantity = 5 (no reservation made) |
| RabbitMQ | `OrderRejectedEvent` published |
| Notification Service | Logs rejection email sent |

### Verification Commands

```bash
# Check order status
curl http://localhost:5000/api/orders/<ORDER_ID>

# Check product stock (should be unchanged)
curl http://localhost:5000/api/products/<PRODUCT_ID>

# Check Notification Service logs in Aspire Dashboard
# Look for: "Sending order rejection email to test@example.com"
```

---

## 2. Stock Low Alert Flow

### Description
When stock falls below the configured threshold during order processing, a `StockLowEvent` should be published and notification sent.

### Prerequisites
- Product with stock just above threshold (e.g., stockQuantity = 10, lowStockThreshold = 5)

### Test Steps

```bash
# 1. Create product with stock near threshold
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Threshold Test Item",
    "description": "Product for stock low testing",
    "price": 49.99,
    "stockQuantity": 10,
    "lowStockThreshold": 5,
    "category": "test"
  }'
# Save the returned productId

# 2. Create order that will drop stock below threshold
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "22222222-2222-2222-2222-222222222222",
    "customerEmail": "buyer@example.com",
    "items": [
      {
        "productId": "<PRODUCT_ID>",
        "quantity": 6
      }
    ]
  }'
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 201 Created |
| Order status | `"status": "Confirmed"` |
| Product stock | stockQuantity = 4 (below threshold of 5) |
| RabbitMQ events | `OrderConfirmedEvent` + `StockLowEvent` published |
| Notification Service | Logs confirmation email + stock low alert |

### Verification Commands

```bash
# Check product stock (should be 4)
curl http://localhost:5000/api/products/<PRODUCT_ID>

# Check RabbitMQ Management UI for StockLowEvent
# Open from Aspire Dashboard → RabbitMQ

# Check Notification Service logs
# Look for: "Stock low alert: Threshold Test Item"
```

---

## 3. Correlation ID Propagation Flow

### Description
Every request should carry a correlation ID through the entire service chain for distributed tracing.

### Prerequisites
- All services running
- Access to service logs via Aspire Dashboard

### Test Steps

```bash
# 1. Create order with explicit correlation ID header
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "X-Correlation-Id: test-correlation-12345" \
  -d '{
    "customerId": "33333333-3333-3333-3333-333333333333",
    "customerEmail": "trace@example.com",
    "items": [
      {
        "productId": "<EXISTING_PRODUCT_ID>",
        "quantity": 1
      }
    ]
  }'
```

### Expected Results

| Service | Log Contains |
|---------|-------------|
| Gateway | `CorrelationId: test-correlation-12345` |
| Order Service | `CorrelationId: test-correlation-12345` |
| Product Service (gRPC) | `CorrelationId: test-correlation-12345` |
| Notification Service | `CorrelationId: test-correlation-12345` |

### Verification Steps

1. Open Aspire Dashboard
2. Navigate to **Traces** section
3. Search for trace with correlation ID `test-correlation-12345`
4. Verify trace spans all services:
   - Gateway (HTTP request)
   - Order Service (HTTP + gRPC client)
   - Product Service (gRPC server)
   - Notification Service (MassTransit consumer)

### Alternative: Log Verification

```bash
# In Aspire Dashboard → Logs, filter by:
# - Text contains: "test-correlation-12345"
# - Should see logs from all 4 services with same correlation ID
```

---

## 4. Test Results Template

Use this template to document validation results:

```markdown
## Validation Results - [DATE]

### Order Rejection Flow
- [ ] HTTP 200 response received
- [ ] Status = Rejected
- [ ] Stock unchanged
- [ ] OrderRejectedEvent published
- [ ] Rejection notification sent
- **Issues found:** (none / describe)

### Stock Low Alert Flow
- [ ] Order confirmed
- [ ] Stock decreased below threshold
- [ ] StockLowEvent published
- [ ] Stock low notification sent
- **Issues found:** (none / describe)

### Correlation ID Propagation
- [ ] Gateway logs correlation ID
- [ ] Order Service logs correlation ID
- [ ] Product Service logs correlation ID
- [ ] Notification Service logs correlation ID
- [ ] Trace visible in Aspire traces
- **Issues found:** (none / describe)

### Summary
- Total scenarios: 3
- Passed: X
- Failed: X
- Notes: ...
```

---

## 5. Troubleshooting

### Order not rejected
- Check if product exists and has correct stock quantity
- Verify gRPC communication between Order and Product service
- Check Product Service logs for ReserveStock handler

### StockLowEvent not published
- Verify `lowStockThreshold` is set on product
- Check if outbox processor is running in Product Service
- Check RabbitMQ for exchange bindings

### Correlation ID missing
- Verify `X-Correlation-Id` header is passed
- Check if `UseCorrelationIdFilters` is registered in MassTransit config
- Verify Gateway YARP configuration forwards headers
