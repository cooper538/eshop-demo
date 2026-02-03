#!/bin/bash
# Scale up all Container Apps by activating their latest revisions
# Usage: ./scale-up.sh [resource-group] [prefix]

set -e

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"
APPS="gateway product-service order-service notification-service analytics-service rabbitmq"

echo "Scaling up Container Apps in resource group: $RG"

for app in $APPS; do
  FULL_NAME="${PREFIX}-${app}"
  REVISIONS=$(az containerapp revision list \
    --name "$FULL_NAME" \
    --resource-group "$RG" \
    --query "[?!properties.active].name" -o tsv 2>/dev/null)

  if [ -z "$REVISIONS" ]; then
    echo "  ${FULL_NAME}: all revisions already active"
  else
    while IFS= read -r rev; do
      echo "  ${FULL_NAME}: activating ${rev}..."
      az containerapp revision activate \
        --name "$FULL_NAME" \
        --resource-group "$RG" \
        --revision "$rev" \
        --output none
    done <<< "$REVISIONS"
  fi
done

echo "Done. All apps activated"
