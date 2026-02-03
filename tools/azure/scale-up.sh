#!/bin/bash
# Scale up all Container Apps to min=1 replicas
# Usage: ./scale-up.sh [resource-group]

set -e

RG="${1:-eshop-rg}"
APPS="gateway product-service order-service notification-service analytics-service"

echo "Scaling up Container Apps in resource group: $RG"

for app in $APPS; do
  echo "  Setting $app min-replicas=1..."
  az containerapp update \
    --name "$app" \
    --resource-group "$RG" \
    --min-replicas 1 \
    --output none
done

echo "Done. All apps have min-replicas=1"
