#!/bin/bash
# Warm up the demo environment before presentation
# Usage: ./warm-up.sh [resource-group] [gateway-url]

set -e

RG="${1:-eshop-rg}"
GATEWAY_URL="${2:-https://gateway.eshop-env.westeurope.azurecontainerapps.io}"

echo "=== EShop Demo Warm-up ==="
echo "Resource group: $RG"
echo "Gateway URL: $GATEWAY_URL"
echo ""

# Step 1: Start PostgreSQL if stopped
echo "[1/4] Starting PostgreSQL..."
az postgres flexible-server start \
  --resource-group "$RG" \
  --name "${RG//-rg/}-postgres" \
  --output none 2>/dev/null || echo "  (already running or not found)"

# Step 2: Start RabbitMQ if stopped
echo "[2/4] Starting RabbitMQ..."
az container start \
  --resource-group "$RG" \
  --name "${RG//-rg/}-rabbitmq" \
  --output none 2>/dev/null || echo "  (already running or not found)"

# Step 3: Scale up Container Apps
echo "[3/4] Scaling up Container Apps..."
"$(dirname "$0")/scale-up.sh" "$RG"

# Step 4: Wait and warm cache
echo "[4/4] Warming up endpoints..."
sleep 10

echo "  Checking health..."
curl -s -o /dev/null -w "  GET /health: %{http_code}\n" "$GATEWAY_URL/health" || true

echo "  Warming API cache..."
curl -s -o /dev/null -w "  GET /api/products: %{http_code}\n" "$GATEWAY_URL/api/products" || true

echo ""
echo "=== Warm-up complete ==="
echo "Demo is ready at: $GATEWAY_URL"
