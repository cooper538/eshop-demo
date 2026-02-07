#!/bin/bash
# Ensure infrastructure is ready for deployment
# Starts PostgreSQL and RabbitMQ if they were stopped by nightly hibernate
# Usage: ./ensure-infra-ready.sh [resource-group] [prefix]

set -e

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"

echo "=== Ensuring infrastructure is ready for deployment ==="

# Start PostgreSQL if stopped
PG_NAME=$(az postgres flexible-server list \
  --resource-group "$RG" \
  --query "[?starts_with(name, '${PREFIX}')].name | [0]" -o tsv 2>/dev/null) || true

if [ -n "$PG_NAME" ]; then
  PG_STATE=$(az postgres flexible-server show \
    --resource-group "$RG" \
    --name "$PG_NAME" \
    --query "state" -o tsv 2>/dev/null) || true

  echo "[1/2] PostgreSQL '$PG_NAME' (state: ${PG_STATE:-unknown})"
  if [[ "$PG_STATE" == "Stopped" ]]; then
    echo "  Starting..."
    az postgres flexible-server start \
      --resource-group "$RG" \
      --name "$PG_NAME" \
      --output none
    echo "  Started successfully"
  else
    echo "  Already running"
  fi
else
  echo "[1/2] No PostgreSQL server found (first deployment?), skipping"
fi

# Wake up RabbitMQ if scaled to 0
RABBITMQ_APP="${PREFIX}-rabbitmq"

CURRENT_MIN=$(az containerapp show \
  --name "$RABBITMQ_APP" \
  --resource-group "$RG" \
  --query "properties.template.scale.minReplicas" -o tsv 2>/dev/null) || true

echo "[2/2] RabbitMQ '$RABBITMQ_APP' (min-replicas: ${CURRENT_MIN:-unknown})"
if [[ "$CURRENT_MIN" == "0" ]]; then
  echo "  Activating revision..."
  INACTIVE_REV=$(az containerapp revision list \
    --name "$RABBITMQ_APP" \
    --resource-group "$RG" \
    --query "[?!properties.active].name" -o tsv 2>/dev/null) || true

  if [ -n "$INACTIVE_REV" ]; then
    while IFS= read -r rev; do
      az containerapp revision activate \
        --name "$RABBITMQ_APP" \
        --resource-group "$RG" \
        --revision "$rev" \
        --output none
    done <<< "$INACTIVE_REV"
  fi

  echo "  Setting min-replicas=1..."
  az containerapp update \
    --name "$RABBITMQ_APP" \
    --resource-group "$RG" \
    --min-replicas 1 \
    --output none

  echo "  Waiting 30s for RabbitMQ to be ready..."
  sleep 30
  echo "  RabbitMQ is up"
else
  echo "  Already running"
fi

echo "=== Infrastructure is ready ==="
