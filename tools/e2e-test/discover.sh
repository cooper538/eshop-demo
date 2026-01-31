#!/bin/bash
# Service Discovery Script for E2E Testing
# Finds running Aspire services and their dynamic ports

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  E-SHOP SERVICE DISCOVERY${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo ""

# Find services by process name (supports multiple patterns)
find_service_port() {
    local service_pattern="$1"
    # Try to find port matching any of the patterns (pipe-separated)
    local port=$(lsof -i -P -n 2>/dev/null | grep LISTEN | grep -E "$service_pattern" | grep -v "Rider\|ReSharper" | head -1 | awk '{print $9}' | sed 's/.*://' | head -1)
    echo "$port"
}

# Find Docker container port mapping
find_container_port() {
    local container_pattern="$1"
    local internal_port="$2"
    docker ps --filter "name=$container_pattern" --format "{{.Ports}}" 2>/dev/null | grep -oE "127\.0\.0\.1:[0-9]+->$internal_port" | cut -d: -f2 | cut -d- -f1
}

# Check health endpoint
check_health() {
    local url="$1"
    local response=$(curl -s -o /dev/null -w "%{http_code}" --max-time 2 "$url/health" 2>/dev/null || echo "000")
    if [ "$response" = "200" ]; then
        echo -e "${GREEN}✓ Healthy${NC}"
    else
        echo -e "${RED}✗ Unhealthy (HTTP $response)${NC}"
    fi
}

echo -e "${YELLOW}SERVICES${NC}"
echo "─────────────────────────────────────────────────────────"

# Gateway (old: Gateway.A, new: EShop.Gat)
GATEWAY_PORT=$(find_service_port "Gateway\.A|EShop\.Gat")
if [ -n "$GATEWAY_PORT" ]; then
    echo -e "Gateway:      ${GREEN}http://localhost:$GATEWAY_PORT${NC} $(check_health "http://localhost:$GATEWAY_PORT")"
    echo "GATEWAY_URL=http://localhost:$GATEWAY_PORT" > "$SCRIPT_DIR/.env"
    echo "GATEWAY_PORT=$GATEWAY_PORT" >> "$SCRIPT_DIR/.env"
else
    echo -e "Gateway:      ${RED}Not running${NC}"
    echo "# Gateway not found" > "$SCRIPT_DIR/.env"
fi

# Order API (old: Order.API, new: EShop.Ord)
ORDER_PORT=$(find_service_port "Order\.API|EShop\.Ord")
if [ -n "$ORDER_PORT" ]; then
    echo -e "Order API:    ${GREEN}http://localhost:$ORDER_PORT${NC} $(check_health "http://localhost:$ORDER_PORT")"
    echo "ORDER_URL=http://localhost:$ORDER_PORT" >> "$SCRIPT_DIR/.env"
    echo "ORDER_PORT=$ORDER_PORT" >> "$SCRIPT_DIR/.env"
else
    echo -e "Order API:    ${RED}Not running${NC}"
fi

# Product API (old: Products., new: EShop.Pro)
PRODUCT_PORT=$(find_service_port "Products\.|EShop\.Pro")
if [ -n "$PRODUCT_PORT" ]; then
    echo -e "Product API:  ${GREEN}http://localhost:$PRODUCT_PORT${NC} $(check_health "http://localhost:$PRODUCT_PORT")"
    echo "PRODUCT_URL=http://localhost:$PRODUCT_PORT" >> "$SCRIPT_DIR/.env"
    echo "PRODUCT_PORT=$PRODUCT_PORT" >> "$SCRIPT_DIR/.env"
else
    echo -e "Product API:  ${RED}Not running${NC}"
fi

echo ""
echo -e "${YELLOW}INFRASTRUCTURE${NC}"
echo "─────────────────────────────────────────────────────────"

# PostgreSQL
PG_CONTAINER=$(docker ps --filter "name=postgres" --format "{{.Names}}" 2>/dev/null | grep -v "pgadmin" | head -1)
if [ -n "$PG_CONTAINER" ]; then
    PG_PORT=$(find_container_port "postgres" "5432")
    PG_PASSWORD=$(docker exec "$PG_CONTAINER" printenv POSTGRES_PASSWORD 2>/dev/null || echo "unknown")
    echo -e "PostgreSQL:   ${GREEN}localhost:$PG_PORT${NC}"
    echo -e "              Container: $PG_CONTAINER"
    echo -e "              Password: $PG_PASSWORD"
    echo "PG_PORT=$PG_PORT" >> "$SCRIPT_DIR/.env"
    echo "PG_CONTAINER=$PG_CONTAINER" >> "$SCRIPT_DIR/.env"
    echo "PG_PASSWORD='$PG_PASSWORD'" >> "$SCRIPT_DIR/.env"
else
    echo -e "PostgreSQL:   ${RED}Not running${NC}"
fi

# PgAdmin
PGADMIN_PORT=$(find_container_port "pgadmin" "80")
if [ -n "$PGADMIN_PORT" ]; then
    echo -e "PgAdmin:      ${GREEN}http://localhost:$PGADMIN_PORT${NC}"
fi

# RabbitMQ
RMQ_CONTAINER=$(docker ps --filter "name=messaging" --format "{{.Names}}" 2>/dev/null | head -1)
if [ -n "$RMQ_CONTAINER" ]; then
    RMQ_PORT=$(find_container_port "messaging" "5672")
    RMQ_MGMT=$(find_container_port "messaging" "15672")
    echo -e "RabbitMQ:     ${GREEN}localhost:$RMQ_PORT${NC} (AMQP)"
    if [ -n "$RMQ_MGMT" ]; then
        echo -e "              ${GREEN}http://localhost:$RMQ_MGMT${NC} (Management)"
    fi
    echo "RMQ_PORT=$RMQ_PORT" >> "$SCRIPT_DIR/.env"
    echo "RMQ_MGMT_PORT=$RMQ_MGMT" >> "$SCRIPT_DIR/.env"
else
    echo -e "RabbitMQ:     ${RED}Not running${NC}"
fi

echo ""
echo -e "${YELLOW}DATABASES${NC}"
echo "─────────────────────────────────────────────────────────"

if [ -n "$PG_CONTAINER" ]; then
    DBS=$(docker exec "$PG_CONTAINER" bash -c 'PGPASSWORD="$POSTGRES_PASSWORD" psql -U postgres -t -c "SELECT datname FROM pg_database WHERE datname NOT IN ('"'"'postgres'"'"', '"'"'template0'"'"', '"'"'template1'"'"');"' 2>/dev/null | tr -d ' ')
    for db in $DBS; do
        if [ -n "$db" ]; then
            echo -e "  ${GREEN}$db${NC}"
        fi
    done
fi

echo ""
echo -e "${YELLOW}ASPIRE DASHBOARD${NC}"
echo "─────────────────────────────────────────────────────────"
DASHBOARD_PORT=$(lsof -i -P -n 2>/dev/null | grep LISTEN | grep "dotnet" | grep -v "Rider" | awk '{print $9}' | grep "1510\|1519\|1520" | sed 's/.*://' | sort -u | head -1)
if [ -n "$DASHBOARD_PORT" ]; then
    echo -e "Dashboard:    ${GREEN}http://localhost:$DASHBOARD_PORT${NC}"
else
    echo -e "Dashboard:    ${YELLOW}Check AppHost output for URL${NC}"
fi

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo -e "Environment saved to: ${SCRIPT_DIR}/.env"
echo -e "Usage: source ${SCRIPT_DIR}/.env"
echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
