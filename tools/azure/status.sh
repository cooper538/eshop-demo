#!/bin/bash
# Check status of all Azure resources
# Usage: ./status.sh [resource-group]

set -e

RG="${1:-eshop-rg}"

echo "=== EShop Demo Status ==="
echo "Resource group: $RG"
echo ""

# PostgreSQL
echo "[PostgreSQL]"
PG_STATE=$(az postgres flexible-server show \
  --resource-group "$RG" \
  --name "${RG//-rg/}-postgres" \
  --query "state" -o tsv 2>/dev/null || echo "not-found")
echo "  State: $PG_STATE"

# RabbitMQ (ACI)
echo ""
echo "[RabbitMQ]"
RMQ_STATE=$(az container show \
  --resource-group "$RG" \
  --name "${RG//-rg/}-rabbitmq" \
  --query "instanceView.state" -o tsv 2>/dev/null || echo "not-found")
echo "  State: $RMQ_STATE"

# Container Apps
echo ""
echo "[Container Apps]"
APPS="gateway product-service order-service notification-service analytics-service"
for app in $APPS; do
  REPLICAS=$(az containerapp show \
    --name "$app" \
    --resource-group "$RG" \
    --query "properties.template.scale.minReplicas" -o tsv 2>/dev/null || echo "?")
  RUNNING=$(az containerapp replica list \
    --name "$app" \
    --resource-group "$RG" \
    --query "length(@)" -o tsv 2>/dev/null || echo "?")
  printf "  %-20s min=%s running=%s\n" "$app" "$REPLICAS" "$RUNNING"
done

echo ""
echo "=== Cost Estimate ==="
if [[ "$PG_STATE" == "Ready" ]]; then
  echo "  PostgreSQL: ~\$12/month (running)"
else
  echo "  PostgreSQL: ~\$4/month (stopped)"
fi

if [[ "$RMQ_STATE" == "Running" ]]; then
  echo "  RabbitMQ:   ~\$10/month (running)"
else
  echo "  RabbitMQ:   \$0/month (stopped)"
fi

echo "  ACR + misc: ~\$5/month"
