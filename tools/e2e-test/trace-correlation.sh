#!/bin/bash
# Trace Correlation ID across all services
# Usage: ./trace-correlation.sh <correlation-id> [--all-logs]
# Example: ./trace-correlation.sh 228617a4-175a-4384-a8e2-ade916a78c3f
#
# Searches for a CorrelationId across all service logs and displays
# a unified, chronologically sorted trace of the request flow.
#
# Options:
#   --all-logs    Search all log files, not just the latest per service
#   --json        Output as JSON (for programmatic use)

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
GRAY='\033[0;90m'
NC='\033[0m'
BOLD='\033[1m'

# Parse arguments
CORRELATION_ID=""
SEARCH_ALL_LOGS=false
OUTPUT_JSON=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --all-logs)
            SEARCH_ALL_LOGS=true
            shift
            ;;
        --json)
            OUTPUT_JSON=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 <correlation-id> [--all-logs] [--json]"
            echo ""
            echo "Trace a request across all microservices using CorrelationId."
            echo ""
            echo "Arguments:"
            echo "  correlation-id   GUID to trace (e.g., 228617a4-175a-4384-a8e2-ade916a78c3f)"
            echo ""
            echo "Options:"
            echo "  --all-logs       Search all log files, not just the latest per service"
            echo "  --json           Output as JSON for programmatic use"
            echo "  -h, --help       Show this help message"
            echo ""
            echo "Example:"
            echo "  $0 228617a4-175a-4384-a8e2-ade916a78c3f"
            echo "  $0 228617a4-175a-4384-a8e2-ade916a78c3f --all-logs"
            exit 0
            ;;
        *)
            if [[ -z "$CORRELATION_ID" ]]; then
                CORRELATION_ID="$1"
            fi
            shift
            ;;
    esac
done

# Validate CorrelationId
if [[ -z "$CORRELATION_ID" ]]; then
    echo -e "${RED}Error: CorrelationId is required${NC}"
    echo "Usage: $0 <correlation-id> [--all-logs] [--json]"
    exit 1
fi

# Validate GUID format (loose check)
if ! echo "$CORRELATION_ID" | grep -qE '^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$'; then
    echo -e "${YELLOW}Warning: CorrelationId doesn't match standard GUID format${NC}"
fi

# Service definitions (name:color:log_dir)
SERVICES="gateway:${CYAN}:$PROJECT_ROOT/src/Services/Gateway/Gateway.API/logs
order:${GREEN}:$PROJECT_ROOT/src/Services/Order/Order.API/logs
product:${YELLOW}:$PROJECT_ROOT/src/Services/Products/Products.API/logs
notification:${MAGENTA}:$PROJECT_ROOT/src/Services/Notification/logs
analytics:${BLUE}:$PROJECT_ROOT/src/Services/Analytics/logs"

get_latest_log() {
    local log_dir="$1"
    if [ -d "$log_dir" ]; then
        ls -t "$log_dir"/*.log 2>/dev/null | head -1
    fi
}

get_all_logs() {
    local log_dir="$1"
    if [ -d "$log_dir" ]; then
        ls -t "$log_dir"/*.log 2>/dev/null
    fi
}

get_service_color() {
    local service="$1"
    case "$service" in
        gateway) echo "${CYAN}" ;;
        order) echo "${GREEN}" ;;
        product) echo "${YELLOW}" ;;
        notification) echo "${MAGENTA}" ;;
        analytics) echo "${BLUE}" ;;
        *) echo "${NC}" ;;
    esac
}

# Create temp file for aggregated results
TEMP_FILE=$(mktemp)
trap "rm -f $TEMP_FILE" EXIT

# Search each service's logs
echo "$SERVICES" | while IFS=: read -r service color log_dir; do
    if [ ! -d "$log_dir" ]; then
        continue
    fi

    if $SEARCH_ALL_LOGS; then
        log_files=$(get_all_logs "$log_dir")
    else
        log_files=$(get_latest_log "$log_dir")
    fi

    for log_file in $log_files; do
        if [ -f "$log_file" ]; then
            # Search for CorrelationId in two formats:
            # 1. [CorrelationId] in the log template
            # 2. "CorrelationId: xxx" in the message
            grep -i "$CORRELATION_ID" "$log_file" 2>/dev/null | while IFS= read -r line; do
                # Extract timestamp for sorting (format: [HH:mm:ss])
                timestamp=$(echo "$line" | grep -oE '^\[[0-9]{2}:[0-9]{2}:[0-9]{2}' | tr -d '[')
                if [ -n "$timestamp" ]; then
                    # Prefix with timestamp and service name for sorting
                    echo "${timestamp}|${service}|${line}" >> "$TEMP_FILE"
                fi
            done
        fi
    done
done

# Count results
TOTAL_LINES=$(wc -l < "$TEMP_FILE" | tr -d ' ')

if [ "$TOTAL_LINES" -eq 0 ]; then
    if ! $OUTPUT_JSON; then
        echo -e "${RED}═══════════════════════════════════════════════════════════════════════${NC}"
        echo -e "${RED}  No logs found for CorrelationId: $CORRELATION_ID${NC}"
        echo -e "${RED}═══════════════════════════════════════════════════════════════════════${NC}"
        echo ""
        echo "Tips:"
        echo "  - Try using --all-logs to search all log files (not just latest)"
        echo "  - Verify the CorrelationId is correct"
        echo "  - Check if the services were running when the request was made"
    else
        echo '{"correlationId":"'"$CORRELATION_ID"'","found":false,"entries":[]}'
    fi
    exit 0
fi

# Sort by timestamp and display
if $OUTPUT_JSON; then
    echo '{"correlationId":"'"$CORRELATION_ID"'","found":true,"totalEntries":'"$TOTAL_LINES"',"entries":['
    first=true
    sort "$TEMP_FILE" | while IFS='|' read -r timestamp service line; do
        # Extract log level
        level=$(echo "$line" | grep -oE '\[INF\]|\[WRN\]|\[ERR\]|\[DBG\]|\[VRB\]' | tr -d '[]')
        level=${level:-"INF"}

        # Escape JSON special characters
        escaped_line=$(echo "$line" | sed 's/\\/\\\\/g; s/"/\\"/g; s/	/\\t/g')

        if $first; then
            first=false
        else
            echo ","
        fi
        echo -n '{"timestamp":"'"$timestamp"'","service":"'"$service"'","level":"'"$level"'","message":"'"$escaped_line"'"}'
    done
    echo ']}'
else
    echo -e "${BOLD}═══════════════════════════════════════════════════════════════════════${NC}"
    echo -e "${BOLD}  CORRELATION TRACE: ${CYAN}$CORRELATION_ID${NC}"
    echo -e "${BOLD}═══════════════════════════════════════════════════════════════════════${NC}"
    echo -e "${GRAY}Found $TOTAL_LINES log entries across services${NC}"
    echo ""

    # Print service legend
    echo -e "${BOLD}Services:${NC}"
    echo -e "  ${CYAN}■${NC} gateway"
    echo -e "  ${GREEN}■${NC} order"
    echo -e "  ${YELLOW}■${NC} product"
    echo -e "  ${MAGENTA}■${NC} notification"
    echo -e "  ${BLUE}■${NC} analytics"
    echo ""
    echo -e "${BOLD}───────────────────────────────────────────────────────────────────────${NC}"
    echo ""

    # Sort and display with colors
    sort "$TEMP_FILE" | while IFS='|' read -r timestamp service line; do
        color=$(get_service_color "$service")
        service_padded=$(printf "%-12s" "$service")

        # Colorize log level in the line
        if echo "$line" | grep -q "ERR"; then
            echo -e "${color}[${service_padded}]${NC} ${RED}$line${NC}"
        elif echo "$line" | grep -q "WRN"; then
            echo -e "${color}[${service_padded}]${NC} ${YELLOW}$line${NC}"
        else
            echo -e "${color}[${service_padded}]${NC} $line"
        fi
    done

    echo ""
    echo -e "${BOLD}───────────────────────────────────────────────────────────────────────${NC}"
    echo -e "${GRAY}Total: $TOTAL_LINES entries${NC}"
fi
