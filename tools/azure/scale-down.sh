#!/bin/bash
# Scale down all Container Apps by setting min-replicas=0 and deactivating revisions
# Usage: ./scale-down.sh [resource-group] [prefix]

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"
APPS="gateway product-service order-service notification-service analytics-service rabbitmq"
MAX_RETRIES=5
RETRY_DELAY=15
ERRORS=0

echo "Scaling down Container Apps in resource group: $RG"

for app in $APPS; do
  FULL_NAME="${PREFIX}-${app}"

  # Set min-replicas=0 with retry for in-progress operations
  echo "  ${FULL_NAME}: setting min-replicas=0..."
  SUCCEEDED=false
  for attempt in $(seq 1 $MAX_RETRIES); do
    OUTPUT=$(az containerapp update \
      --name "$FULL_NAME" \
      --resource-group "$RG" \
      --min-replicas 0 \
      --output none 2>&1) && { SUCCEEDED=true; break; }

    if echo "$OUTPUT" | grep -q "ContainerAppOperationInProgress"; then
      echo "  ${FULL_NAME}: provisioning in progress, retry ${attempt}/${MAX_RETRIES} (waiting ${RETRY_DELAY}s)..."
      sleep $RETRY_DELAY
    else
      echo "  ${FULL_NAME}: ERROR: $OUTPUT"
      break
    fi
  done

  if [[ "$SUCCEEDED" != true ]]; then
    echo "  ${FULL_NAME}: FAILED to scale down"
    ERRORS=$((ERRORS + 1))
    continue
  fi

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
        --output none 2>&1 || echo "  ${FULL_NAME}: WARNING: failed to deactivate ${rev}"
    done <<< "$REVISIONS"
  fi
done

if [ $ERRORS -gt 0 ]; then
  echo "Done with $ERRORS error(s). Some apps may not be scaled down."
  exit 1
fi

echo "Done. All apps scaled to zero and deactivated"
