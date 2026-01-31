#!/bin/bash
# Hibernate the demo environment to minimize costs
# Usage: ./hibernate.sh [resource-group]

set -e

RG="${1:-eshop-rg}"

echo "=== EShop Demo Hibernate ==="
echo "Resource group: $RG"
echo ""
echo "This will stop PostgreSQL and RabbitMQ to minimize costs."
echo "Estimated cost after hibernation: ~\$9/month"
echo ""
read -p "Continue? (y/N) " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
  echo "Cancelled."
  exit 0
fi

# Step 1: Scale down Container Apps
echo "[1/3] Scaling down Container Apps..."
"$(dirname "$0")/scale-down.sh" "$RG"

# Step 2: Stop RabbitMQ
echo "[2/3] Stopping RabbitMQ..."
az container stop \
  --resource-group "$RG" \
  --name "${RG//-rg/}-rabbitmq" \
  --output none 2>/dev/null || echo "  (not found or already stopped)"

# Step 3: Stop PostgreSQL
echo "[3/3] Stopping PostgreSQL..."
az postgres flexible-server stop \
  --resource-group "$RG" \
  --name "${RG//-rg/}-postgres" \
  --output none 2>/dev/null || echo "  (not found or already stopped)"

echo ""
echo "=== Hibernation complete ==="
echo "Monthly cost: ~\$9 (ACR + storage only)"
echo ""
echo "To wake up, run: ./warm-up.sh $RG"
