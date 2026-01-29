#!/bin/bash
# Database Query Helper for E2E Testing
# Usage: ./db-query.sh <database> "<SQL query>"
# Example: ./db-query.sh orderdb "SELECT * FROM \"Order\" LIMIT 5;"

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Load environment if available
if [ -f "$SCRIPT_DIR/.env" ]; then
    source "$SCRIPT_DIR/.env"
fi

# Find PostgreSQL container if not in env
if [ -z "$PG_CONTAINER" ]; then
    PG_CONTAINER=$(docker ps --filter "name=postgres" --format "{{.Names}}" 2>/dev/null | grep -v "pgadmin" | head -1)
fi

if [ -z "$PG_CONTAINER" ]; then
    echo "Error: PostgreSQL container not found. Is Aspire running?"
    exit 1
fi

DATABASE="${1:-productdb}"
QUERY="${2:-SELECT 1;}"

# Execute query using stdin to preserve special characters
echo "$QUERY" | docker exec -i "$PG_CONTAINER" bash -c 'PGPASSWORD="$POSTGRES_PASSWORD" psql -U postgres -d "'"$DATABASE"'"'
