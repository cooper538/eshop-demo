#!/bin/bash
# Wake up the demo environment by triggering deploy workflows
# Usage: ./warm-up.sh [resource-group] [prefix]

set -e

RG="${1:-eshop-prod-rg}"
PREFIX="${2:-eshop}"

echo "=== EShop Demo Wake-up ==="

# Step 1: Start PostgreSQL
echo "[1/3] Starting PostgreSQL..."
"$(dirname "$0")/ensure-infra-ready.sh" "$RG" "$PREFIX"

# Step 2: Trigger Infrastructure workflow (recreates Container Apps)
echo "[2/3] Triggering Infrastructure workflow..."
gh workflow run "Infrastructure" --field skip_what_if=true
echo "  Triggered. Waiting for completion..."
sleep 10
gh run watch --exit-status $(gh run list --workflow=infra.yml --limit 1 --json databaseId --jq '.[0].databaseId')

# Step 3: Trigger Application workflow (deploys latest images)
echo "[3/3] Triggering Application workflow..."
gh workflow run "Application"
echo "  Triggered. Monitor at: gh run watch"

echo ""
echo "=== Wake-up initiated ==="
echo "Monitor progress: gh run list"
