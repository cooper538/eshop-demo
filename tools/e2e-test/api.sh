#!/bin/bash
# API Helper for E2E Testing
# Usage: ./api.sh <command> [args]
#
# Commands:
#   products              - List all products
#   product <id>          - Get product by ID
#   orders                - List all orders
#   order <id>            - Get order by ID
#   create-order <json>   - Create new order
#   cancel-order <id>     - Cancel order

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Load environment
if [ -f "$SCRIPT_DIR/.env" ]; then
    source "$SCRIPT_DIR/.env"
fi

# Auto-discover if not in env
if [ -z "$ORDER_URL" ]; then
    ORDER_PORT=$(lsof -i -P -n 2>/dev/null | grep LISTEN | grep "Order.API" | head -1 | awk '{print $9}' | sed 's/.*://')
    ORDER_URL="http://localhost:$ORDER_PORT"
fi

if [ -z "$PRODUCT_URL" ]; then
    PRODUCT_PORT=$(lsof -i -P -n 2>/dev/null | grep LISTEN | grep "Products." | head -1 | awk '{print $9}' | sed 's/.*://')
    PRODUCT_URL="http://localhost:$PRODUCT_PORT"
fi

if [ -z "$GATEWAY_URL" ]; then
    GATEWAY_PORT=$(lsof -i -P -n 2>/dev/null | grep LISTEN | grep "Gateway.A" | head -1 | awk '{print $9}' | sed 's/.*://')
    GATEWAY_URL="http://localhost:$GATEWAY_PORT"
fi

COMMAND="${1:-help}"
shift || true

case "$COMMAND" in
    products)
        echo "GET $PRODUCT_URL/api/products"
        curl -s "$PRODUCT_URL/api/products" | jq '.'
        ;;
    product)
        ID="$1"
        echo "GET $PRODUCT_URL/api/products/$ID"
        curl -s "$PRODUCT_URL/api/products/$ID" | jq '.'
        ;;
    orders)
        echo "GET $ORDER_URL/api/orders"
        curl -s "$ORDER_URL/api/orders" | jq '.'
        ;;
    order)
        ID="$1"
        echo "GET $ORDER_URL/api/orders/$ID"
        curl -s "$ORDER_URL/api/orders/$ID" | jq '.'
        ;;
    create-order)
        JSON="$1"
        if [ -z "$JSON" ]; then
            # Default test order
            PRODUCT_ID=$(curl -s "$PRODUCT_URL/api/products" | jq -r '.items[0].id')
            PRODUCT_NAME=$(curl -s "$PRODUCT_URL/api/products" | jq -r '.items[0].name')
            PRODUCT_PRICE=$(curl -s "$PRODUCT_URL/api/products" | jq -r '.items[0].price')
            CUSTOMER_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')

            JSON=$(cat <<EOF
{
    "customerId": "$CUSTOMER_ID",
    "customerEmail": "test@example.com",
    "items": [{
        "productId": "$PRODUCT_ID",
        "productName": "$PRODUCT_NAME",
        "quantity": 1,
        "unitPrice": $PRODUCT_PRICE
    }]
}
EOF
)
        fi
        echo "POST $ORDER_URL/api/orders"
        echo "$JSON" | jq '.'
        echo "---"
        curl -s -X POST "$ORDER_URL/api/orders" \
            -H "Content-Type: application/json" \
            -d "$JSON" | jq '.'
        ;;
    cancel-order)
        ID="$1"
        echo "POST $ORDER_URL/api/orders/$ID/cancel"
        curl -s -X POST "$ORDER_URL/api/orders/$ID/cancel" | jq '.'
        ;;
    health)
        echo "Health Checks:"
        echo -n "  Gateway:  "; curl -s -o /dev/null -w "%{http_code}" "$GATEWAY_URL/health" 2>/dev/null || echo "DOWN"
        echo ""
        echo -n "  Order:    "; curl -s -o /dev/null -w "%{http_code}" "$ORDER_URL/health" 2>/dev/null || echo "DOWN"
        echo ""
        echo -n "  Product:  "; curl -s -o /dev/null -w "%{http_code}" "$PRODUCT_URL/health" 2>/dev/null || echo "DOWN"
        echo ""
        ;;
    urls)
        echo "Service URLs:"
        echo "  Gateway:  $GATEWAY_URL"
        echo "  Order:    $ORDER_URL"
        echo "  Product:  $PRODUCT_URL"
        ;;
    help|*)
        echo "E2E Test API Helper"
        echo ""
        echo "Usage: $0 <command> [args]"
        echo ""
        echo "Commands:"
        echo "  products              - List all products"
        echo "  product <id>          - Get product by ID"
        echo "  orders                - List all orders"
        echo "  order <id>            - Get order by ID"
        echo "  create-order [json]   - Create new order (auto-generates if no JSON)"
        echo "  cancel-order <id>     - Cancel order"
        echo "  health                - Check all health endpoints"
        echo "  urls                  - Show service URLs"
        echo ""
        echo "Examples:"
        echo "  $0 products"
        echo "  $0 create-order"
        echo "  $0 order abc-123-def"
        exit 1
        ;;
esac
