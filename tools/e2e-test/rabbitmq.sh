#!/bin/bash
# RabbitMQ Diagnostics for E2E Testing
# Usage: ./rabbitmq.sh [command]
#
# Commands:
#   status      - Overview of RabbitMQ state (default)
#   queues      - List all queues with message counts
#   connections - Show active connections
#   consumers   - List consumers per queue
#   exchanges   - List exchanges and bindings
#   messages    - Show pending messages (potential issues)
#   purge <q>   - Purge messages from queue (use with caution)

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Load environment
if [ -f "$SCRIPT_DIR/.env" ]; then
    source "$SCRIPT_DIR/.env"
fi

# Auto-discover RabbitMQ management port
if [ -z "$RMQ_MGMT_PORT" ]; then
    RMQ_MGMT_PORT=$(docker ps --filter "name=messaging" --format "{{.Ports}}" 2>/dev/null | grep -oE "127\.0\.0\.1:[0-9]+->15672" | cut -d: -f2 | cut -d- -f1)
fi

if [ -z "$RMQ_MGMT_PORT" ]; then
    echo "Error: RabbitMQ management port not found. Is Aspire running?"
    exit 1
fi

RMQ_URL="http://localhost:$RMQ_MGMT_PORT"

# Get RabbitMQ credentials from container (Aspire sets custom password)
RMQ_CONTAINER=$(docker ps --filter "name=messaging" --format "{{.Names}}" 2>/dev/null | head -1)
if [ -n "$RMQ_CONTAINER" ]; then
    RMQ_USER=$(docker exec "$RMQ_CONTAINER" printenv RABBITMQ_DEFAULT_USER 2>/dev/null || echo "guest")
    RMQ_PASS=$(docker exec "$RMQ_CONTAINER" printenv RABBITMQ_DEFAULT_PASS 2>/dev/null || echo "guest")
else
    RMQ_USER="${RMQ_USER:-guest}"
    RMQ_PASS="${RMQ_PASS:-guest}"
fi

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

rmq_api() {
    curl -s -u "$RMQ_USER:$RMQ_PASS" "$RMQ_URL/api/$1" 2>/dev/null
}

cmd_status() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  RABBITMQ STATUS${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    # Overview
    local overview=$(rmq_api "overview")
    local node=$(echo "$overview" | jq -r '.node // "unknown"')
    local version=$(echo "$overview" | jq -r '.rabbitmq_version // "unknown"')
    local msg_ready=$(echo "$overview" | jq -r '.queue_totals.messages_ready // 0')
    local msg_unacked=$(echo "$overview" | jq -r '.queue_totals.messages_unacknowledged // 0')
    local connections=$(echo "$overview" | jq -r '.object_totals.connections // 0')
    local queues=$(echo "$overview" | jq -r '.object_totals.queues // 0')
    local consumers=$(echo "$overview" | jq -r '.object_totals.consumers // 0')

    echo -e "${YELLOW}CLUSTER${NC}"
    echo "─────────────────────────────────────────────────────────"
    echo -e "  Node:        $node"
    echo -e "  Version:     $version"
    echo -e "  Management:  ${GREEN}$RMQ_URL${NC}"
    echo ""

    echo -e "${YELLOW}TOTALS${NC}"
    echo "─────────────────────────────────────────────────────────"
    echo -e "  Queues:      $queues"
    echo -e "  Connections: $connections"
    echo -e "  Consumers:   $consumers"
    echo ""

    echo -e "${YELLOW}MESSAGES${NC}"
    echo "─────────────────────────────────────────────────────────"
    if [ "$msg_ready" -gt 0 ]; then
        echo -e "  Ready:       ${YELLOW}$msg_ready${NC} (pending processing)"
    else
        echo -e "  Ready:       ${GREEN}$msg_ready${NC}"
    fi
    if [ "$msg_unacked" -gt 0 ]; then
        echo -e "  Unacked:     ${YELLOW}$msg_unacked${NC} (in progress)"
    else
        echo -e "  Unacked:     ${GREEN}$msg_unacked${NC}"
    fi
    echo ""

    # Quick health check
    echo -e "${YELLOW}HEALTH${NC}"
    echo "─────────────────────────────────────────────────────────"
    local health=$(rmq_api "health/checks/alarms")
    if echo "$health" | jq -e '.status == "ok"' > /dev/null 2>&1; then
        echo -e "  Status:      ${GREEN}✓ Healthy${NC}"
    else
        echo -e "  Status:      ${RED}✗ Unhealthy${NC}"
        echo "$health" | jq -r '.reason // "Unknown error"' | sed 's/^/  /'
    fi
    echo ""
}

cmd_queues() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  RABBITMQ QUEUES${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    local queues=$(rmq_api "queues")

    echo "$queues" | jq -r '
        ["QUEUE", "READY", "UNACKED", "TOTAL", "CONSUMERS", "STATE"],
        ["─────", "─────", "──────", "─────", "─────────", "─────"],
        (.[] | [
            .name,
            (.messages_ready // 0 | tostring),
            (.messages_unacknowledged // 0 | tostring),
            (.messages // 0 | tostring),
            (.consumers // 0 | tostring),
            .state
        ]) | @tsv
    ' | column -t -s $'\t'

    echo ""

    # Highlight issues
    local issues=$(echo "$queues" | jq -r '.[] | select(.messages_ready > 0) | .name')
    if [ -n "$issues" ]; then
        echo -e "${YELLOW}⚠️  Queues with pending messages:${NC}"
        echo "$issues" | sed 's/^/  - /'
    fi
}

cmd_connections() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  RABBITMQ CONNECTIONS${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    local conns=$(rmq_api "connections")

    echo "$conns" | jq -r '
        ["CLIENT", "STATE", "CHANNELS", "PROTOCOL"],
        ["──────", "─────", "────────", "────────"],
        (.[] | [
            (.client_properties.connection_name // .user // "unknown"),
            .state,
            (.channels // 0 | tostring),
            .protocol
        ]) | @tsv
    ' | column -t -s $'\t'

    echo ""

    # Expected connections for e-shop
    echo -e "${YELLOW}EXPECTED SERVICES${NC}"
    echo "─────────────────────────────────────────────────────────"
    local conn_names=$(echo "$conns" | jq -r '.[].client_properties.connection_name // empty')

    for service in "order" "notification" "analytics" "product"; do
        if echo "$conn_names" | grep -qi "$service"; then
            echo -e "  $service: ${GREEN}✓ Connected${NC}"
        else
            echo -e "  $service: ${YELLOW}? Not found (may use different name)${NC}"
        fi
    done
}

cmd_consumers() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  RABBITMQ CONSUMERS${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    local consumers=$(rmq_api "consumers")

    echo "$consumers" | jq -r '
        ["QUEUE", "CONSUMER TAG", "PREFETCH", "ACK REQUIRED"],
        ["─────", "────────────", "────────", "────────────"],
        (.[] | [
            .queue.name,
            .consumer_tag,
            (.prefetch_count // 0 | tostring),
            (if .ack_required then "yes" else "no" end)
        ]) | @tsv
    ' | column -t -s $'\t'
}

cmd_exchanges() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  RABBITMQ EXCHANGES & BINDINGS${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    echo -e "${YELLOW}EXCHANGES${NC}"
    echo "─────────────────────────────────────────────────────────"
    local exchanges=$(rmq_api "exchanges")
    echo "$exchanges" | jq -r '.[] | select(.name != "") | "  \(.name) (\(.type))"'
    echo ""

    echo -e "${YELLOW}BINDINGS (non-default)${NC}"
    echo "─────────────────────────────────────────────────────────"
    local bindings=$(rmq_api "bindings")
    echo "$bindings" | jq -r '
        .[] | select(.source != "") |
        "  \(.source) → \(.destination) [key: \(.routing_key // "*")]"
    '
}

cmd_messages() {
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  PENDING MESSAGES ANALYSIS${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""

    local queues=$(rmq_api "queues")
    local total_ready=$(echo "$queues" | jq '[.[].messages_ready] | add // 0')
    local total_unacked=$(echo "$queues" | jq '[.[].messages_unacknowledged] | add // 0')

    if [ "$total_ready" -eq 0 ] && [ "$total_unacked" -eq 0 ]; then
        echo -e "${GREEN}✓ All messages processed - no pending messages${NC}"
        echo ""
        return
    fi

    echo -e "${YELLOW}Summary:${NC}"
    echo "  Ready (waiting):     $total_ready"
    echo "  Unacked (processing): $total_unacked"
    echo ""

    # Show queues with messages
    echo -e "${YELLOW}Queues with pending messages:${NC}"
    echo "$queues" | jq -r '
        .[] | select(.messages > 0) |
        "  \(.name): \(.messages_ready) ready, \(.messages_unacknowledged) unacked"
    '
    echo ""

    # Peek at messages if any ready
    if [ "$total_ready" -gt 0 ]; then
        echo -e "${YELLOW}⚠️  Potential issues:${NC}"
        echo "  - Messages stuck in queue may indicate consumer failures"
        echo "  - Check consumer logs for errors"
        echo "  - Verify consumers are running and connected"
    fi
}

cmd_purge() {
    local queue="$1"
    if [ -z "$queue" ]; then
        echo "Usage: $0 purge <queue-name>"
        exit 1
    fi

    echo -e "${RED}⚠️  Purging queue: $queue${NC}"
    curl -s -u "$RMQ_USER:$RMQ_PASS" -X DELETE "$RMQ_URL/api/queues/%2F/$queue/contents"
    echo -e "${GREEN}Done${NC}"
}

cmd_help() {
    echo "RabbitMQ Diagnostics for E2E Testing"
    echo ""
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  status      - Overview of RabbitMQ state (default)"
    echo "  queues      - List all queues with message counts"
    echo "  connections - Show active connections"
    echo "  consumers   - List consumers per queue"
    echo "  exchanges   - List exchanges and bindings"
    echo "  messages    - Analyze pending messages"
    echo "  purge <q>   - Purge messages from queue"
    echo ""
    echo "Examples:"
    echo "  $0 status"
    echo "  $0 queues"
    echo "  $0 messages"
}

# Main
COMMAND="${1:-status}"
shift || true

case "$COMMAND" in
    status)     cmd_status ;;
    queues)     cmd_queues ;;
    connections) cmd_connections ;;
    consumers)  cmd_consumers ;;
    exchanges)  cmd_exchanges ;;
    messages)   cmd_messages ;;
    purge)      cmd_purge "$@" ;;
    help|--help|-h) cmd_help ;;
    *)
        echo "Unknown command: $COMMAND"
        cmd_help
        exit 1
        ;;
esac
