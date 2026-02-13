#!/bin/bash
# Ensure PostgreSQL is running before deployment
# Usage: ./ensure-infra-ready.sh [resource-group] [prefix]

set -e

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"

PG_NAME=$(az postgres flexible-server list --resource-group "$RG" --query "[?starts_with(name, '${PREFIX}')].name | [0]" -o tsv 2>/dev/null) || true

if [ -n "$PG_NAME" ]; then
  PG_STATE=$(az postgres flexible-server show --resource-group "$RG" --name "$PG_NAME" --query "state" -o tsv 2>/dev/null) || true
  echo "PostgreSQL '$PG_NAME' (state: ${PG_STATE:-unknown})"
  if [[ "$PG_STATE" == "Stopped" ]]; then
    echo "  Starting..."
    az postgres flexible-server start --resource-group "$RG" --name "$PG_NAME" --output none
    echo "  Started"
  else
    echo "  Already running"
  fi
else
  echo "No PostgreSQL server found (first deployment?), skipping"
fi
