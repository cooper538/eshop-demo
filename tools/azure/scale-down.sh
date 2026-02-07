#!/bin/bash
# Scale down all Container Apps by setting min-replicas=0 and deactivating revisions
# Usage: ./scale-down.sh [resource-group] [prefix]

set -e

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"
APPS="gateway product-service order-service notification-service analytics-service rabbitmq"

echo "Scaling down Container Apps in resource group: $RG"

for app in $APPS; do
  FULL_NAME="${PREFIX}-${app}"

  # Set min-replicas=0 so Azure doesn't restart the app
  echo "  ${FULL_NAME}: setting min-replicas=0..."
  az containerapp update \
    --name "$FULL_NAME" \
    --resource-group "$RG" \
    --min-replicas 0 \
    --output none

  # Deactivate active revisions
  REVISIONS=$(az containerapp revision list \
    --name "$FULL_NAME" \
    --resource-group "$RG" \
    --query "[?properties.active].name" -o tsv 2>/dev/null)

  if [ -z "$REVISIONS" ]; then
    echo "  ${FULL_NAME}: no active revision found"
  else
    while IFS= read -r rev; do
      echo "  ${FULL_NAME}: deactivating ${rev}..."
      az containerapp revision deactivate \
        --name "$FULL_NAME" \
        --resource-group "$RG" \
        --revision "$rev" \
        --output none
    done <<< "$REVISIONS"
  fi
done

echo "Done. All apps scaled to zero and deactivated"
