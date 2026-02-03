#!/bin/bash
# Scale up all Container Apps by activating revisions and setting min-replicas=1
# Usage: ./scale-up.sh [resource-group] [prefix]

set -e

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"
APPS="gateway product-service order-service notification-service analytics-service rabbitmq"

echo "Scaling up Container Apps in resource group: $RG"

for app in $APPS; do
  FULL_NAME="${PREFIX}-${app}"

  # Activate inactive revisions
  REVISIONS=$(az containerapp revision list \
    --name "$FULL_NAME" \
    --resource-group "$RG" \
    --query "[?!properties.active].name" -o tsv 2>/dev/null)

  if [ -n "$REVISIONS" ]; then
    while IFS= read -r rev; do
      echo "  ${FULL_NAME}: activating ${rev}..."
      az containerapp revision activate \
        --name "$FULL_NAME" \
        --resource-group "$RG" \
        --revision "$rev" \
        --output none
    done <<< "$REVISIONS"
  fi

  # Ensure min-replicas=1 so the app actually starts
  echo "  ${FULL_NAME}: setting min-replicas=1..."
  az containerapp update \
    --name "$FULL_NAME" \
    --resource-group "$RG" \
    --min-replicas 1 \
    --output none
done

echo "Done. All apps activated with min-replicas=1"
