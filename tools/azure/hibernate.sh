#!/bin/bash
# Hibernate the demo environment to minimize costs
# Usage: ./hibernate.sh [resource-group] [prefix] [--yes]

set -e

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"
AUTO_CONFIRM=false

for arg in "$@"; do
  if [[ "$arg" == "--yes" || "$arg" == "-y" ]]; then
    AUTO_CONFIRM=true
  fi
done

echo "=== EShop Demo Hibernate ==="
echo "Resource group: $RG"
echo ""
echo "This will scale down all Container Apps and stop PostgreSQL to minimize costs."
echo "Estimated cost after hibernation: ~\$9/month"
echo ""

if [[ "$AUTO_CONFIRM" != true ]]; then
  read -p "Continue? (y/N) " -n 1 -r
  echo ""

  if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Cancelled."
    exit 0
  fi
fi

ERRORS=0

# Step 1: Scale down Container Apps (includes RabbitMQ)
echo "[1/2] Scaling down Container Apps..."
"$(dirname "$0")/scale-down.sh" "$RG" "$PREFIX" || ERRORS=$((ERRORS + 1))

# Step 2: Stop PostgreSQL
PG_NAME=$(az postgres flexible-server list \
  --resource-group "$RG" \
  --query "[0].name" -o tsv 2>/dev/null)

if [ -z "$PG_NAME" ]; then
  echo "[2/2] Stopping PostgreSQL..."
  echo "  ERROR: No PostgreSQL server found in $RG"
  ERRORS=$((ERRORS + 1))
else
  PG_STATE=$(az postgres flexible-server show \
    --resource-group "$RG" \
    --name "$PG_NAME" \
    --query "state" -o tsv 2>/dev/null) || true

  echo "[2/2] Stopping PostgreSQL ($PG_NAME, state: ${PG_STATE:-unknown})..."
  if [[ "$PG_STATE" == "Stopped" ]]; then
    echo "  Already stopped"
  else
    az postgres flexible-server stop \
      --resource-group "$RG" \
      --name "$PG_NAME" \
      --output none || { echo "  ERROR: Failed to stop PostgreSQL"; ERRORS=$((ERRORS + 1)); }
  fi
fi

# Verify final state
echo ""
echo "=== Verification ==="
"$(dirname "$0")/status.sh" "$RG" "$PREFIX"

if [ $ERRORS -gt 0 ]; then
  echo ""
  echo "WARNING: $ERRORS error(s) occurred during hibernation"
  exit 1
fi

echo ""
echo "To wake up, run: ./warm-up.sh $RG $PREFIX"
