#!/bin/bash
# Warm up the demo environment before presentation
# Usage: ./warm-up.sh [resource-group] [prefix] [gateway-url]

set -e

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"
GATEWAY_URL="${3:-https://gateway.eshop-env.westeurope.azurecontainerapps.io}"

echo "=== EShop Demo Warm-up ==="
echo "Resource group: $RG"
echo "Gateway URL: $GATEWAY_URL"
echo ""

ERRORS=0

# Step 1: Start PostgreSQL if stopped
PG_NAME=$(az postgres flexible-server list \
  --resource-group "$RG" \
  --query "[0].name" -o tsv 2>/dev/null)

if [ -z "$PG_NAME" ]; then
  echo "[1/3] Starting PostgreSQL..."
  echo "  ERROR: No PostgreSQL server found in $RG"
  ERRORS=$((ERRORS + 1))
else
  PG_STATE=$(az postgres flexible-server show \
    --resource-group "$RG" \
    --name "$PG_NAME" \
    --query "state" -o tsv 2>/dev/null) || true

  echo "[1/3] Starting PostgreSQL ($PG_NAME, state: ${PG_STATE:-unknown})..."
  if [[ "$PG_STATE" == "Ready" ]]; then
    echo "  Already running"
  else
    az postgres flexible-server start \
      --resource-group "$RG" \
      --name "$PG_NAME" \
      --output none || { echo "  ERROR: Failed to start PostgreSQL"; ERRORS=$((ERRORS + 1)); }
  fi
fi

# Step 2: Scale up Container Apps (includes RabbitMQ)
echo "[2/3] Scaling up Container Apps..."
"$(dirname "$0")/scale-up.sh" "$RG" "$PREFIX" || ERRORS=$((ERRORS + 1))

# Step 3: Wait and warm cache
echo "[3/3] Warming up endpoints..."
sleep 10

echo "  Checking health..."
curl -s -o /dev/null -w "  GET /health: %{http_code}\n" "$GATEWAY_URL/health" || true

echo "  Warming API cache..."
curl -s -o /dev/null -w "  GET /api/products: %{http_code}\n" "$GATEWAY_URL/api/products" || true

# Verify final state
echo ""
echo "=== Verification ==="
"$(dirname "$0")/status.sh" "$RG" "$PREFIX"

if [ $ERRORS -gt 0 ]; then
  echo ""
  echo "WARNING: $ERRORS error(s) occurred during warm-up"
  exit 1
fi

echo ""
echo "Demo is ready at: $GATEWAY_URL"
