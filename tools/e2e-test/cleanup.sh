#!/bin/bash
# Cleanup Script for E2E Testing
# Usage: ./cleanup.sh [command]
#
# Commands:
#   data        - Reset test data only (orders, reservations, messages)
#   queues      - Purge all RabbitMQ queues
#   logs        - Clean old log files (older than 7 days)
#   env         - Remove generated .env file
#   all         - All of the above
#   status      - Show what would be cleaned

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Load environment
if [ -f "$SCRIPT_DIR/.env" ]; then
    source "$SCRIPT_DIR/.env"
fi

# Find PostgreSQL container
find_pg_container() {
    docker ps --filter "name=postgres" --format "{{.Names}}" 2>/dev/null | grep -v "pgadmin" | head -1
}

db_query() {
    local db="$1"
    local query="$2"
    local container=$(find_pg_container)
    if [ -n "$container" ]; then
        echo "$query" | docker exec -i "$container" bash -c 'PGPASSWORD="$POSTGRES_PASSWORD" psql -U postgres -d "'"$db"'"' 2>/dev/null
    fi
}

cmd_status() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  CLEANUP STATUS${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    # Database stats
    echo -e "${YELLOW}DATABASE${NC}"
    echo "─────────────────────────────────────────────────────────"

    local container=$(find_pg_container)
    if [ -n "$container" ]; then
        local orders=$(db_query orderdb 'SELECT COUNT(*) FROM "Order";' | grep -E '^\s*[0-9]+' | tr -d ' ')
        local reservations=$(db_query productdb 'SELECT COUNT(*) FROM "StockReservation";' | grep -E '^\s*[0-9]+' | tr -d ' ')
        local processed=$(db_query notificationdb 'SELECT COUNT(*) FROM "ProcessedMessage";' | grep -E '^\s*[0-9]+' | tr -d ' ')

        echo "  Orders:           ${orders:-0}"
        echo "  Stock Reservations: ${reservations:-0}"
        echo "  Processed Messages: ${processed:-0}"
    else
        echo -e "  ${RED}PostgreSQL not running${NC}"
    fi
    echo ""

    # RabbitMQ stats
    echo -e "${YELLOW}RABBITMQ${NC}"
    echo "─────────────────────────────────────────────────────────"
    if [ -f "$SCRIPT_DIR/rabbitmq.sh" ]; then
        local rmq_msgs=$("$SCRIPT_DIR/rabbitmq.sh" messages 2>/dev/null | grep -E "Ready|Unacked" | head -2)
        if [ -n "$rmq_msgs" ]; then
            echo "$rmq_msgs" | sed 's/^/  /'
        else
            echo "  No pending messages"
        fi
    fi
    echo ""

    # Log files
    echo -e "${YELLOW}LOG FILES${NC}"
    echo "─────────────────────────────────────────────────────────"
    local log_count=$(find "$PROJECT_ROOT/src/Services" -name "*.log" 2>/dev/null | wc -l | tr -d ' ')
    local old_log_count=$(find "$PROJECT_ROOT/src/Services" -name "*.log" -mtime +7 2>/dev/null | wc -l | tr -d ' ')
    echo "  Total log files:    $log_count"
    echo "  Older than 7 days:  $old_log_count"
    echo ""

    # Env file
    echo -e "${YELLOW}GENERATED FILES${NC}"
    echo "─────────────────────────────────────────────────────────"
    if [ -f "$SCRIPT_DIR/.env" ]; then
        echo -e "  .env file: ${GREEN}exists${NC}"
    else
        echo "  .env file: not present"
    fi
}

cmd_data() {
    echo -e "${YELLOW}Cleaning test data...${NC}"

    local container=$(find_pg_container)
    if [ -z "$container" ]; then
        echo -e "${RED}PostgreSQL not running${NC}"
        return 1
    fi

    echo "  Clearing orders..."
    db_query orderdb 'DELETE FROM "OrderItem"; DELETE FROM "Order"; DELETE FROM "OutboxMessage"; DELETE FROM "OutboxState"; DELETE FROM "InboxState";' > /dev/null

    echo "  Clearing stock reservations..."
    db_query productdb 'DELETE FROM "StockReservation";' > /dev/null

    echo "  Resetting stock quantities..."
    db_query productdb 'UPDATE "Stock" SET "Quantity" = 100;' > /dev/null

    echo "  Clearing processed messages..."
    db_query notificationdb 'DELETE FROM "ProcessedMessage";' > /dev/null

    echo -e "${GREEN}✓ Test data cleared${NC}"
}

cmd_queues() {
    echo -e "${YELLOW}Purging RabbitMQ queues...${NC}"

    if [ ! -f "$SCRIPT_DIR/rabbitmq.sh" ]; then
        echo -e "${RED}rabbitmq.sh not found${NC}"
        return 1
    fi

    # Get queue names
    local queues=$("$SCRIPT_DIR/rabbitmq.sh" queues 2>/dev/null | grep -E "^[a-z]" | awk '{print $1}')

    for queue in $queues; do
        echo "  Purging $queue..."
        "$SCRIPT_DIR/rabbitmq.sh" purge "$queue" > /dev/null 2>&1 || true
    done

    echo -e "${GREEN}✓ Queues purged${NC}"
}

cmd_logs() {
    echo -e "${YELLOW}Cleaning old log files (older than 7 days)...${NC}"

    local count=$(find "$PROJECT_ROOT/src/Services" -name "*.log" -mtime +7 2>/dev/null | wc -l | tr -d ' ')

    if [ "$count" -gt 0 ]; then
        find "$PROJECT_ROOT/src/Services" -name "*.log" -mtime +7 -delete 2>/dev/null
        echo -e "${GREEN}✓ Deleted $count old log files${NC}"
    else
        echo "  No old log files to delete"
    fi
}

cmd_env() {
    echo -e "${YELLOW}Removing generated .env file...${NC}"

    if [ -f "$SCRIPT_DIR/.env" ]; then
        rm -f "$SCRIPT_DIR/.env"
        echo -e "${GREEN}✓ .env file removed${NC}"
    else
        echo "  .env file not present"
    fi
}

cmd_all() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  FULL CLEANUP${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    cmd_data
    echo ""
    cmd_queues
    echo ""
    cmd_logs
    echo ""
    cmd_env
    echo ""

    echo -e "${GREEN}═══════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}  Cleanup complete!${NC}"
    echo -e "${GREEN}═══════════════════════════════════════════════════════${NC}"
}

cmd_help() {
    echo "E2E Test Cleanup Script"
    echo ""
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  status    - Show what would be cleaned (default)"
    echo "  data      - Reset test data (orders, reservations, messages)"
    echo "  queues    - Purge all RabbitMQ queues"
    echo "  logs      - Clean old log files (older than 7 days)"
    echo "  env       - Remove generated .env file"
    echo "  all       - All of the above"
    echo ""
    echo "Examples:"
    echo "  $0 status    # See what needs cleaning"
    echo "  $0 data      # Clear test orders"
    echo "  $0 all       # Full cleanup"
}

# Main
COMMAND="${1:-all}"

case "$COMMAND" in
    status)   cmd_status ;;
    data)     cmd_data ;;
    queues)   cmd_queues ;;
    logs)     cmd_logs ;;
    env)      cmd_env ;;
    all)      cmd_all ;;
    help|--help|-h) cmd_help ;;
    *)
        echo "Unknown command: $COMMAND"
        cmd_help
        exit 1
        ;;
esac
