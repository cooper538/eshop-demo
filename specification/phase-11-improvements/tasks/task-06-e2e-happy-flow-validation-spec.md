# E2E Happy Flow Validation - Test Specification

## Overview

This document describes complete test scenarios for validating all happy flows in the e-shop system. Each scenario includes prerequisites, test steps, and expected outcomes.

---

## 1. Create Order Flow (Happy Path)

### Description
Customer creates an order with sufficient stock. Order is confirmed, stock is reserved, and confirmation notification is sent.

### Prerequisites
- All services running via `dotnet run --project src/AppHost`
- Product with sufficient stock exists

### Test Steps

```bash
# 1. Create a product with sufficient stock
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Product",
    "description": "Product for order testing",
    "price": 149.99,
    "stockQuantity": 100,
    "lowStockThreshold": 10,
    "category": "test"
  }'
# Save the returned productId

# 2. Create order
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerEmail": "customer@example.com",
    "items": [
      {
        "productId": "<PRODUCT_ID>",
        "quantity": 2
      }
    ]
  }'
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 201 Created |
| Response status | `"status": "Confirmed"` |
| Order in DB | Status = Confirmed |
| Product stock | Decreased by 2 (98 remaining) |
| Stock Reservation | Created with Status = Active |
| RabbitMQ | `OrderConfirmedEvent` published |
| Notification Service | Logs confirmation email sent |

### Verification Commands

```bash
# Check order status
curl http://localhost:5000/api/orders/<ORDER_ID>

# Check product stock (should be decreased)
curl http://localhost:5000/api/products/<PRODUCT_ID>

# Check Notification Service logs in Aspire Dashboard
# Look for: "Sending order confirmation email to customer@example.com"
```

---

## 2. Cancel Order Flow

### Description
Customer cancels a confirmed order. Stock is released back to inventory and cancellation notification is sent.

### Prerequisites
- Confirmed order exists (from scenario 1)

### Test Steps

```bash
# Cancel the order created in scenario 1
curl -X POST http://localhost:5000/api/orders/<ORDER_ID>/cancel \
  -H "Content-Type: application/json"
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 200 OK |
| Response status | `"status": "Cancelled"` |
| Order in DB | Status = Cancelled |
| Product stock | Increased back (100 again) |
| Stock Reservation | Status = Released |
| RabbitMQ | `OrderCancelledEvent` published |
| Notification Service | Logs cancellation email sent |

### Verification Commands

```bash
# Check order status
curl http://localhost:5000/api/orders/<ORDER_ID>

# Check product stock (should be restored)
curl http://localhost:5000/api/products/<PRODUCT_ID>

# Check Notification Service logs
# Look for: "Sending order cancellation email"
```

---

## 3. Order Rejection Flow

### Description
When a customer attempts to order more items than available in stock, the order should be rejected with appropriate notification.

### Prerequisites
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
    "customerId": "22222222-2222-2222-2222-222222222222",
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

## 4. Stock Low Alert Flow

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
    "customerId": "33333333-3333-3333-3333-333333333333",
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

## 5. Correlation ID Propagation Flow

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
    "customerId": "44444444-4444-4444-4444-444444444444",
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

## 6. Test Results Template

Use this template to document validation results:

```markdown
## Validation Results - [DATE]

### 1. Create Order Flow
- [ ] HTTP 201 response received
- [ ] Status = Confirmed
- [ ] Stock decreased
- [ ] OrderConfirmedEvent published
- [ ] Confirmation notification sent
- **Issues found:** (none / describe)

### 2. Cancel Order Flow
- [ ] HTTP 200 response received
- [ ] Status = Cancelled
- [ ] Stock restored
- [ ] OrderCancelledEvent published
- [ ] Cancellation notification sent
- **Issues found:** (none / describe)

### 3. Order Rejection Flow
- [ ] HTTP 200 response received
- [ ] Status = Rejected
- [ ] Stock unchanged
- [ ] OrderRejectedEvent published
- [ ] Rejection notification sent
- **Issues found:** (none / describe)

### 4. Stock Low Alert Flow
- [ ] Order confirmed
- [ ] Stock decreased below threshold
- [ ] StockLowEvent published
- [ ] Stock low notification sent
- **Issues found:** (none / describe)

### 5. Correlation ID Propagation
- [ ] Gateway logs correlation ID
- [ ] Order Service logs correlation ID
- [ ] Product Service logs correlation ID
- [ ] Notification Service logs correlation ID
- [ ] Trace visible in Aspire traces
- **Issues found:** (none / describe)

### Summary
- Total scenarios: 5
- Passed: X
- Failed: X
- Notes: ...
```

---

## 7. Troubleshooting

### Order not confirmed
- Check if product exists and has sufficient stock
- Verify gRPC communication between Order and Product service
- Check Product Service logs for ReserveStock handler

### Order not rejected
- Check if product exists and has correct stock quantity
- Verify the requested quantity exceeds available stock

### Stock not released on cancel
- Check if order was in Confirmed state
- Verify gRPC ReleaseStock call in logs
- Check Stock Reservation status in database

### StockLowEvent not published
- Verify `lowStockThreshold` is set on product
- Check if outbox processor is running in Product Service
- Check RabbitMQ for exchange bindings

### Correlation ID missing
- Verify `X-Correlation-Id` header is passed
- Check if `UseCorrelationIdFilters` is registered in MassTransit config
- Verify Gateway YARP configuration forwards headers
