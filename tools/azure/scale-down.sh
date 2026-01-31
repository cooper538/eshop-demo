#!/bin/bash
# Scale down all Container Apps to min=0 replicas (scale-to-zero)
# Usage: ./scale-down.sh [resource-group]

set -e

RG="${1:-eshop-rg}"
APPS="gateway product-service order-service notification-service catalog-service"

echo "Scaling down Container Apps in resource group: $RG"

for app in $APPS; do
  echo "  Setting $app min-replicas=0..."
  az containerapp update \
    --name "$app" \
    --resource-group "$RG" \
    --min-replicas 0 \
    --output none
done

echo "Done. All apps have min-replicas=0 (scale-to-zero enabled)"
