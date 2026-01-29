#!/bin/bash
# Log Reader for E2E Testing
# Usage: ./logs.sh <service> [lines]
# Example: ./logs.sh order 50
# Services: order, product, gateway, notification, analytics

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

SERVICE="${1:-all}"
LINES="${2:-30}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

get_latest_log() {
    local log_dir="$1"
    if [ -d "$log_dir" ]; then
        ls -t "$log_dir"/*.log 2>/dev/null | head -1
    fi
}

print_logs() {
    local service_name="$1"
    local log_dir="$2"
    local log_file=$(get_latest_log "$log_dir")

    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${YELLOW}  $service_name LOGS${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"

    if [ -n "$log_file" ] && [ -f "$log_file" ]; then
        echo -e "${GREEN}File: $log_file${NC}"
        echo "─────────────────────────────────────────────────────────"
        tail -n "$LINES" "$log_file" | while IFS= read -r line; do
            # Colorize log levels
            if echo "$line" | grep -q "ERR"; then
                echo -e "${RED}$line${NC}"
            elif echo "$line" | grep -q "WRN"; then
                echo -e "${YELLOW}$line${NC}"
            else
                echo "$line"
            fi
        done
    else
        echo -e "${RED}No log files found in $log_dir${NC}"
    fi
    echo ""
}

case "$SERVICE" in
    order|Order)
        print_logs "ORDER API" "$PROJECT_ROOT/src/Services/Order/Order.API/logs"
        ;;
    product|Product|products)
        print_logs "PRODUCT API" "$PROJECT_ROOT/src/Services/Products/Products.API/logs"
        ;;
    gateway|Gateway)
        print_logs "GATEWAY" "$PROJECT_ROOT/src/Services/Gateway/Gateway.API/logs"
        ;;
    notification|Notification)
        print_logs "NOTIFICATION" "$PROJECT_ROOT/src/Services/Notification/logs"
        ;;
    analytics|Analytics)
        print_logs "ANALYTICS" "$PROJECT_ROOT/src/Services/Analytics/logs"
        ;;
    all)
        print_logs "ORDER API" "$PROJECT_ROOT/src/Services/Order/Order.API/logs"
        print_logs "PRODUCT API" "$PROJECT_ROOT/src/Services/Products/Products.API/logs"
        print_logs "GATEWAY" "$PROJECT_ROOT/src/Services/Gateway/Gateway.API/logs"
        print_logs "NOTIFICATION" "$PROJECT_ROOT/src/Services/Notification/logs"
        print_logs "ANALYTICS" "$PROJECT_ROOT/src/Services/Analytics/logs"
        ;;
    errors)
        echo -e "${RED}═══════════════════════════════════════════════════════${NC}"
        echo -e "${RED}  ERROR LOG SUMMARY${NC}"
        echo -e "${RED}═══════════════════════════════════════════════════════${NC}"
        for dir in "$PROJECT_ROOT/src/Services/"*/logs "$PROJECT_ROOT/src/Services/"/*/*/logs; do
            if [ -d "$dir" ]; then
                log_file=$(get_latest_log "$dir")
                if [ -n "$log_file" ]; then
                    errors=$(grep -c "ERR" "$log_file" 2>/dev/null || echo "0")
                    warnings=$(grep -c "WRN" "$log_file" 2>/dev/null || echo "0")
                    service=$(echo "$dir" | sed 's|.*/Services/||' | sed 's|/logs||' | sed 's|/.*||')
                    if [ "$errors" -gt 0 ] || [ "$warnings" -gt 0 ]; then
                        echo -e "$service: ${RED}$errors errors${NC}, ${YELLOW}$warnings warnings${NC}"
                        grep "ERR" "$log_file" 2>/dev/null | tail -3 | sed 's/^/  /'
                    fi
                fi
            fi
        done
        ;;
    *)
        echo "Usage: $0 <service> [lines]"
        echo ""
        echo "Services:"
        echo "  order       - Order API logs"
        echo "  product     - Product API logs"
        echo "  gateway     - Gateway logs"
        echo "  notification - Notification service logs"
        echo "  analytics   - Analytics service logs"
        echo "  all         - All service logs"
        echo "  errors      - Only errors from all services"
        echo ""
        echo "Example: $0 order 50"
        exit 1
        ;;
esac
