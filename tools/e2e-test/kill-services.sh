#!/bin/bash
# Kill all running EShop services
# Usage: ./kill-services.sh [--force]
#
# Options:
#   --force    Kill without confirmation

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

FORCE=false
if [ "$1" = "--force" ] || [ "$1" = "-f" ]; then
    FORCE=true
fi

# Find EShop processes
find_eshop_processes() {
    # Match EShop.*, Order.API, Products.API, Gateway.API, etc.
    pgrep -f "EShop\." 2>/dev/null || true
    pgrep -f "Order\.API" 2>/dev/null || true
    pgrep -f "Products\.API" 2>/dev/null || true
    pgrep -f "Gateway\.API" 2>/dev/null || true
    pgrep -f "Notification\.API" 2>/dev/null || true
    pgrep -f "Analytics\.API" 2>/dev/null || true
    pgrep -f "DatabaseMigration" 2>/dev/null || true
}

# Get unique PIDs
get_pids() {
    find_eshop_processes | sort -u
}

# Show running processes
show_processes() {
    local pids=$(get_pids)

    if [ -z "$pids" ]; then
        echo -e "${GREEN}No EShop services running${NC}"
        return 1
    fi

    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  RUNNING ESHOP SERVICES${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    echo -e "${YELLOW}PID       CPU    MEM    COMMAND${NC}"
    echo "─────────────────────────────────────────────────────────"

    for pid in $pids; do
        if ps -p "$pid" > /dev/null 2>&1; then
            ps -p "$pid" -o pid=,pcpu=,pmem=,comm= 2>/dev/null | while read p cpu mem cmd; do
                printf "%-9s %-6s %-6s %s\n" "$p" "$cpu%" "$mem%" "$cmd"
            done
        fi
    done

    echo ""
    local count=$(echo "$pids" | wc -w | tr -d ' ')
    echo -e "Total: ${YELLOW}$count${NC} process(es)"

    return 0
}

# Kill all EShop processes
kill_processes() {
    local pids=$(get_pids)

    if [ -z "$pids" ]; then
        echo -e "${GREEN}No EShop services to kill${NC}"
        return 0
    fi

    local count=$(echo "$pids" | wc -w | tr -d ' ')

    if [ "$FORCE" = false ]; then
        echo -e "${YELLOW}About to kill $count EShop process(es)${NC}"
        echo ""
        show_processes
        echo ""
        read -p "Continue? [y/N] " -n 1 -r
        echo ""
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            echo "Aborted."
            return 1
        fi
    fi

    echo -e "${YELLOW}Killing EShop services...${NC}"

    # First try graceful termination (SIGTERM)
    for pid in $pids; do
        if ps -p "$pid" > /dev/null 2>&1; then
            echo "  Sending SIGTERM to PID $pid..."
            kill "$pid" 2>/dev/null || true
        fi
    done

    # Wait a moment for graceful shutdown
    sleep 2

    # Check if any processes still running and force kill
    local remaining=$(get_pids)
    if [ -n "$remaining" ]; then
        echo -e "${YELLOW}Some processes still running, sending SIGKILL...${NC}"
        for pid in $remaining; do
            if ps -p "$pid" > /dev/null 2>&1; then
                echo "  Force killing PID $pid..."
                kill -9 "$pid" 2>/dev/null || true
            fi
        done
    fi

    echo ""
    echo -e "${GREEN}✓ All EShop services killed${NC}"
}

# Help
show_help() {
    echo "Kill all running EShop services"
    echo ""
    echo "Usage: $0 [options] [command]"
    echo ""
    echo "Commands:"
    echo "  status    - Show running services (default if no args)"
    echo "  kill      - Kill all services (default with --force)"
    echo ""
    echo "Options:"
    echo "  -f, --force    Kill without confirmation"
    echo ""
    echo "Examples:"
    echo "  $0              # Show running services"
    echo "  $0 kill         # Kill with confirmation"
    echo "  $0 --force      # Kill without confirmation"
    echo "  $0 -f kill      # Same as above"
}

# Main
COMMAND="${1:-status}"

# Handle --force as first arg
if [ "$1" = "--force" ] || [ "$1" = "-f" ]; then
    COMMAND="${2:-kill}"
fi

case "$COMMAND" in
    status)
        show_processes
        ;;
    kill)
        kill_processes
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        echo "Unknown command: $COMMAND"
        show_help
        exit 1
        ;;
esac
