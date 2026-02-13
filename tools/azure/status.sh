#!/bin/bash
# Check status of all Azure resources
# Usage: ./status.sh [resource-group] [prefix]

set -e

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"

echo "=== EShop Demo Status ==="
echo "Resource group: $RG"
echo ""

# PostgreSQL
echo "[PostgreSQL]"
PG_NAME=$(az postgres flexible-server list \
  --resource-group "$RG" \
  --query "[0].name" -o tsv 2>/dev/null)

if [ -n "$PG_NAME" ]; then
  PG_STATE=$(az postgres flexible-server show \
    --resource-group "$RG" \
    --name "$PG_NAME" \
    --query "state" -o tsv 2>/dev/null || echo "not-found")
  echo "  Server: $PG_NAME"
  echo "  State: $PG_STATE"
else
  PG_STATE="not-found"
  echo "  State: not-found"
fi

# Container Apps (includes RabbitMQ)
echo ""
echo "[Container Apps]"
APPS="gateway product-service order-service notification-service analytics-service rabbitmq"
for app in $APPS; do
  FULL_NAME="${PREFIX}-${app}"
  REPLICAS=$(az containerapp show \
    --name "$FULL_NAME" \
    --resource-group "$RG" \
    --query "properties.template.scale.minReplicas" -o tsv 2>/dev/null || echo "not-found")
  if [[ "$REPLICAS" == "not-found" ]]; then
    printf "  %-30s NOT DEPLOYED\n" "$FULL_NAME"
    continue
  fi
  RUNNING=$(az containerapp replica list \
    --name "$FULL_NAME" \
    --resource-group "$RG" \
    --query "length(@)" -o tsv 2>/dev/null || echo "?")
  printf "  %-30s min=%s running=%s\n" "$FULL_NAME" "$REPLICAS" "$RUNNING"
done

echo ""
echo "=== Cost Estimate ==="
if [[ "$PG_STATE" == "Ready" ]]; then
  echo "  PostgreSQL: ~\$12/month (running)"
else
  echo "  PostgreSQL: ~\$4/month (stopped)"
fi

echo "  Monitoring + misc: ~\$5/month"
