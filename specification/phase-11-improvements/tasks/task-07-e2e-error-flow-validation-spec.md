# E2E Error Flow Validation - Test Specification

## Overview

This document describes test scenarios for validating error handling flows in the e-shop system. Each scenario includes prerequisites, test steps, and expected error responses.

---

## 1. Non-Existent Product in Order

### Description
Customer attempts to create an order with a product ID that doesn't exist.

### Prerequisites
- All services running via `dotnet run --project src/AppHost`

### Test Steps

```bash
# Create order with non-existent product ID
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerEmail": "customer@example.com",
    "items": [
      {
        "productId": "99999999-9999-9999-9999-999999999999",
        "quantity": 1
      }
    ]
  }'
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 404 Not Found |
| Error message | Contains "Product not found" |
| Order created | No order in database |
| Stock unchanged | No reservations made |

---

## 2. Non-Existent Order Lookup

### Description
Attempt to retrieve an order that doesn't exist.

### Test Steps

```bash
# Get order with non-existent ID
curl http://localhost:5000/api/orders/99999999-9999-9999-9999-999999999999
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 404 Not Found |
| Error message | Contains "Order not found" |

---

## 3. Missing Required Fields (400 Bad Request)

### Description
Create order with missing required fields.

### Test Steps

```bash
# 3a. Missing customerId
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerEmail": "customer@example.com",
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "quantity": 1
      }
    ]
  }'

# 3b. Missing items
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerEmail": "customer@example.com",
    "items": []
  }'

# 3c. Missing email
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "quantity": 1
      }
    ]
  }'
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 400 Bad Request |
| Error format | Problem Details JSON (RFC 7807) |
| Validation errors | Lists specific missing fields |

---

## 4. Invalid Data Formats (400 Bad Request)

### Description
Create order with invalid data types or formats.

### Test Steps

```bash
# 4a. Invalid GUID format for customerId
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "not-a-valid-guid",
    "customerEmail": "customer@example.com",
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "quantity": 1
      }
    ]
  }'

# 4b. Invalid email format
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerEmail": "not-an-email",
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "quantity": 1
      }
    ]
  }'

# 4c. Negative quantity
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerEmail": "customer@example.com",
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "quantity": -5
      }
    ]
  }'
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 400 Bad Request |
| Error format | Problem Details JSON (RFC 7807) |
| Validation errors | Lists specific invalid fields |

---

## 5. Duplicate Order Cancellation

### Description
Attempt to cancel an order that has already been cancelled.

### Prerequisites
- Cancelled order exists (use order from happy flow task-06, then cancel it first)

### Test Steps

```bash
# First ensure order is cancelled, then try to cancel again
curl -X POST http://localhost:5000/api/orders/<CANCELLED_ORDER_ID>/cancel \
  -H "Content-Type: application/json"
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 400 Bad Request or 409 Conflict |
| Error message | Contains "already cancelled" or "invalid state" |
| Order status | Remains Cancelled (unchanged) |

---

## 6. Cancellation of Rejected Order

### Description
Attempt to cancel an order that was rejected (never confirmed).

### Prerequisites
- Rejected order exists (from happy flow scenario 3)

### Test Steps

```bash
# Try to cancel a rejected order
curl -X POST http://localhost:5000/api/orders/<REJECTED_ORDER_ID>/cancel \
  -H "Content-Type: application/json"
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 400 Bad Request or 409 Conflict |
| Error message | Contains "cannot cancel" or "invalid state" |
| Order status | Remains Rejected (unchanged) |

---

## 7. Zero Quantity in Order

### Description
Create order with zero quantity.

### Test Steps

```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerEmail": "customer@example.com",
    "items": [
      {
        "productId": "<EXISTING_PRODUCT_ID>",
        "quantity": 0
      }
    ]
  }'
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 400 Bad Request |
| Error message | Contains "quantity" validation error |

---

## 8. Product Service Unavailable (gRPC Error)

### Description
Test behavior when Product Service is unavailable during order creation.

### Prerequisites
- Stop Product Service manually (Ctrl+C in its terminal)
- Keep other services running

### Test Steps

```bash
# Try to create order while Product Service is down
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "customerEmail": "customer@example.com",
    "items": [
      {
        "productId": "11111111-1111-1111-1111-111111111111",
        "quantity": 1
      }
    ]
  }'
```

### Expected Results

| Check | Expected |
|-------|----------|
| HTTP Response | 503 Service Unavailable or 500 Internal Server Error |
| Error message | Indicates service communication failure |
| No partial state | No order created, no stock changes |

### Cleanup
- Restart Product Service

---

## 9. Test Results Template

Use this template to document validation results:

```markdown
## Error Flow Validation Results - [DATE]

### 1. Non-Existent Product
- [ ] HTTP 404 response received
- [ ] Error message mentions product not found
- [ ] No order created
- **Issues found:** (none / describe)

### 2. Non-Existent Order Lookup
- [ ] HTTP 404 response received
- [ ] Error message mentions order not found
- **Issues found:** (none / describe)

### 3. Missing Required Fields
- [ ] HTTP 400 for missing customerId
- [ ] HTTP 400 for empty items
- [ ] HTTP 400 for missing email
- [ ] Problem Details format used
- **Issues found:** (none / describe)

### 4. Invalid Data Formats
- [ ] HTTP 400 for invalid GUID
- [ ] HTTP 400 for invalid email
- [ ] HTTP 400 for negative quantity
- [ ] Problem Details format used
- **Issues found:** (none / describe)

### 5. Duplicate Cancellation
- [ ] HTTP 400/409 response received
- [ ] Error message indicates already cancelled
- [ ] Order status unchanged
- **Issues found:** (none / describe)

### 6. Cancel Rejected Order
- [ ] HTTP 400/409 response received
- [ ] Error message indicates invalid state
- [ ] Order status unchanged
- **Issues found:** (none / describe)

### 7. Zero Quantity
- [ ] HTTP 400 response received
- [ ] Quantity validation error returned
- **Issues found:** (none / describe)

### 8. Product Service Unavailable
- [ ] HTTP 503/500 response received
- [ ] Error indicates service failure
- [ ] No partial state created
- **Issues found:** (none / describe)

### Summary
- Total scenarios: 8
- Passed: X
- Failed: X
- Notes: ...
```

