#!/bin/bash
# gRPC Diagnostics for E2E Testing
# Usage: ./grpc.sh [command]
#
# Commands:
#   status      - Check gRPC endpoints reachability
#   list        - List available gRPC services (requires reflection)
#   test        - Test gRPC call to Product service
#   discovery   - Show service discovery configuration

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Load environment
if [ -f "$SCRIPT_DIR/.env" ]; then
    source "$SCRIPT_DIR/.env"
fi

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Find Product service gRPC port (different from HTTP port)
find_grpc_port() {
    # Product service exposes both HTTP and gRPC on different ports
    # gRPC is typically on the second port found
    local ports=$(lsof -i -P -n 2>/dev/null | grep LISTEN | grep "Products." | grep -v "Rider" | awk '{print $9}' | sed 's/.*://' | sort -u)
    echo "$ports"
}

check_grpcurl() {
    if command -v grpcurl &> /dev/null; then
        return 0
    else
        return 1
    fi
}

cmd_status() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  gRPC SERVICE STATUS${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    echo -e "${YELLOW}PRODUCT SERVICE PORTS${NC}"
    echo "─────────────────────────────────────────────────────────"

    local ports=$(find_grpc_port)
    if [ -z "$ports" ]; then
        echo -e "${RED}Product service not running${NC}"
        return 1
    fi

    local port_array=($ports)
    local http_port="${port_array[0]}"
    local grpc_port="${port_array[1]:-$http_port}"

    echo -e "  HTTP Port:  ${GREEN}$http_port${NC}"
    echo -e "  gRPC Port:  ${GREEN}$grpc_port${NC} (may be same as HTTP with HTTP/2)"
    echo ""

    # Test HTTP health
    echo -e "${YELLOW}HTTP HEALTH CHECK${NC}"
    echo "─────────────────────────────────────────────────────────"
    local http_status=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:$http_port/health" 2>/dev/null || echo "000")
    if [ "$http_status" = "200" ]; then
        echo -e "  http://localhost:$http_port/health → ${GREEN}✓ Healthy${NC}"
    else
        echo -e "  http://localhost:$http_port/health → ${RED}✗ HTTP $http_status${NC}"
    fi
    echo ""

    # Check if grpcurl is available
    echo -e "${YELLOW}gRPC CONNECTIVITY${NC}"
    echo "─────────────────────────────────────────────────────────"

    if check_grpcurl; then
        # Try to connect with grpcurl
        for port in $ports; do
            echo -n "  localhost:$port → "
            if grpcurl -plaintext "localhost:$port" list &>/dev/null; then
                echo -e "${GREEN}✓ gRPC available (reflection enabled)${NC}"
            elif grpcurl -plaintext -connect-timeout 2 "localhost:$port" list 2>&1 | grep -q "Failed to dial"; then
                echo -e "${RED}✗ Connection failed${NC}"
            else
                echo -e "${YELLOW}? gRPC reachable but reflection disabled${NC}"
            fi
        done
    else
        echo -e "  ${YELLOW}grpcurl not installed${NC}"
        echo "  Install: brew install grpcurl"
        echo ""
        echo "  Alternative check using curl HTTP/2:"
        for port in $ports; do
            local http2=$(curl -s -o /dev/null -w "%{http_version}" --http2 "http://localhost:$port" 2>/dev/null || echo "?")
            echo "    localhost:$port HTTP version: $http2"
        done
    fi
    echo ""

    # Show proto definition location
    echo -e "${YELLOW}PROTO DEFINITIONS${NC}"
    echo "─────────────────────────────────────────────────────────"
    local proto_file="$PROJECT_ROOT/src/Common/EShop.Grpc/Protos/product.proto"
    if [ -f "$proto_file" ]; then
        echo -e "  Location: ${GREEN}$proto_file${NC}"
        echo ""
        echo "  Services defined:"
        grep -E "^service " "$proto_file" | sed 's/^/    /'
        echo ""
        echo "  RPC methods:"
        grep -E "^\s+rpc " "$proto_file" | sed 's/^/    /'
    else
        echo -e "  ${RED}Proto file not found${NC}"
    fi
}

cmd_list() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  gRPC SERVICE LISTING${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    if ! check_grpcurl; then
        echo -e "${RED}grpcurl not installed${NC}"
        echo "Install: brew install grpcurl"
        exit 1
    fi

    local ports=$(find_grpc_port)
    for port in $ports; do
        echo -e "${YELLOW}Port $port:${NC}"
        if grpcurl -plaintext "localhost:$port" list 2>/dev/null; then
            echo ""
            echo "Methods:"
            grpcurl -plaintext "localhost:$port" list 2>/dev/null | while read service; do
                if [ -n "$service" ] && [ "$service" != "grpc.reflection.v1.ServerReflection" ] && [ "$service" != "grpc.reflection.v1alpha.ServerReflection" ]; then
                    grpcurl -plaintext "localhost:$port" describe "$service" 2>/dev/null | grep "rpc " | sed 's/^/  /'
                fi
            done
        else
            echo -e "  ${YELLOW}Reflection not enabled or service not available${NC}"
        fi
        echo ""
    done
}

cmd_test() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  gRPC TEST CALLS${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    if ! check_grpcurl; then
        echo -e "${YELLOW}grpcurl not installed - using proto file reference${NC}"
        echo ""
        echo "To test gRPC manually, install grpcurl:"
        echo "  brew install grpcurl"
        echo ""
        echo "Then run:"
        echo '  grpcurl -plaintext -d '"'"'{"product_ids": ["352cc928-3dd1-4787-b948-a37d5c6b95d1"]}'"'"' localhost:PORT product.ProductService/GetProducts'
        return
    fi

    local ports=$(find_grpc_port)
    local grpc_port=$(echo "$ports" | tail -1)

    echo -e "${YELLOW}Testing GetProducts${NC}"
    echo "─────────────────────────────────────────────────────────"
    echo "Request: GetProducts with known product ID"
    echo ""

    # Get a product ID from database
    local product_id="352cc928-3dd1-4787-b948-a37d5c6b95d1"

    local result=$(grpcurl -plaintext \
        -d "{\"product_ids\": [\"$product_id\"]}" \
        "localhost:$grpc_port" \
        product.ProductService/GetProducts 2>&1)

    if echo "$result" | grep -q "Error\|error\|failed"; then
        echo -e "${RED}✗ Call failed:${NC}"
        echo "$result" | sed 's/^/  /'
    else
        echo -e "${GREEN}✓ Call succeeded:${NC}"
        echo "$result" | head -20
    fi
    echo ""

    echo -e "${YELLOW}Testing ReserveStock (dry run info)${NC}"
    echo "─────────────────────────────────────────────────────────"
    echo "This method requires:"
    echo '  {
    "order_id": "uuid",
    "items": [{
      "product_id": "uuid",
      "quantity": 1
    }]
  }'
    echo ""
    echo "Used by Order service during order creation."
}

cmd_discovery() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  SERVICE DISCOVERY CONFIGURATION${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    echo -e "${YELLOW}ORDER SERVICE CONFIGURATION${NC}"
    echo "─────────────────────────────────────────────────────────"

    local config_file="$PROJECT_ROOT/src/Services/Order/Order.API/order.settings.yaml"
    if [ -f "$config_file" ]; then
        echo "Config file: $config_file"
        echo ""
        grep -A5 "ProductService" "$config_file" 2>/dev/null | sed 's/^/  /'
    fi
    echo ""

    echo -e "${YELLOW}gRPC CLIENT REGISTRATION${NC}"
    echo "─────────────────────────────────────────────────────────"
    local client_file="$PROJECT_ROOT/src/Common/EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs"
    if [ -f "$client_file" ]; then
        echo "File: $client_file"
        echo ""
        echo "Key registration:"
        grep -A10 "AddGrpcClient" "$client_file" 2>/dev/null | head -15 | sed 's/^/  /'
    fi
    echo ""

    echo -e "${YELLOW}SERVICE DISCOVERY STATUS${NC}"
    echo "─────────────────────────────────────────────────────────"

    # Check if AddServiceDiscovery is configured for gRPC
    if grep -q "AddServiceDiscovery" "$client_file" 2>/dev/null; then
        echo -e "  gRPC client: ${GREEN}✓ Service Discovery configured${NC}"
    else
        echo -e "  gRPC client: ${RED}✗ Service Discovery NOT configured${NC}"
        echo ""
        echo -e "  ${YELLOW}FIX REQUIRED:${NC}"
        echo "  Add .AddServiceDiscovery() to gRPC client in:"
        echo "  $client_file"
        echo ""
        echo "  Example:"
        echo '    services.AddGrpcClient<ProductServiceClient>(o => { ... })'
        echo '        .AddServiceDiscovery()  // <-- ADD THIS'
        echo '        .ConfigureChannel(...)'
    fi
    echo ""

    echo -e "${YELLOW}ASPIRE ENVIRONMENT VARIABLES${NC}"
    echo "─────────────────────────────────────────────────────────"
    echo "When running via Aspire, these should be set:"
    echo "  services__product-service__https__0 = https://localhost:PORT"
    echo "  services__product-service__http__0  = http://localhost:PORT"
    echo ""
    echo "The 'https+http://product-service' scheme resolves using these."
}

cmd_help() {
    echo "gRPC Diagnostics for E2E Testing"
    echo ""
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  status      - Check gRPC endpoints (default)"
    echo "  list        - List gRPC services (requires grpcurl)"
    echo "  test        - Test gRPC calls"
    echo "  discovery   - Show service discovery config"
    echo ""
    echo "Prerequisites:"
    echo "  brew install grpcurl"
    echo ""
    echo "Examples:"
    echo "  $0 status"
    echo "  $0 discovery"
}

# Make scripts executable
chmod +x "$SCRIPT_DIR"/*.sh 2>/dev/null || true

# Main
COMMAND="${1:-status}"
shift || true

case "$COMMAND" in
    status)     cmd_status ;;
    list)       cmd_list ;;
    test)       cmd_test ;;
    discovery)  cmd_discovery ;;
    help|--help|-h) cmd_help ;;
    *)
        echo "Unknown command: $COMMAND"
        cmd_help
        exit 1
        ;;
esac
