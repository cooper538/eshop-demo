#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Database names (from ResourceNames.cs)
DATABASES=("productdb" "orderdb" "notificationdb")

print_usage() {
    printf "${BLUE}Database Reset Tool${NC}\n"
    printf "====================\n\n"
    printf "Usage: %s [OPTIONS] [DATABASE...]\n\n" "$(basename "$0")"
    printf "Options:\n"
    printf "  -a, --all       Reset all databases (default if no database specified)\n"
    printf "  -c, --container Remove the entire PostgreSQL container and volume\n"
    printf "  -h, --help      Show this help message\n\n"
    printf "Databases:\n"
    printf "  product         Reset productdb\n"
    printf "  order           Reset orderdb\n"
    printf "  notification    Reset notificationdb\n\n"
    printf "Examples:\n"
    printf "  %s                    # Reset all databases\n" "$(basename "$0")"
    printf "  %s product            # Reset only productdb\n" "$(basename "$0")"
    printf "  %s product order      # Reset productdb and orderdb\n" "$(basename "$0")"
    printf "  %s -c                 # Remove container and volume entirely\n" "$(basename "$0")"
}

find_postgres_container() {
    # Aspire names containers with resource name prefix
    docker ps -a --format '{{.Names}}' | grep -E '^postgres' | head -1 || true
}

reset_database() {
    local db_name="$1"
    local container="$2"

    printf "${YELLOW}Resetting ${db_name}...${NC}\n"

    # Drop and recreate the database
    docker exec "$container" psql -U postgres -c "DROP DATABASE IF EXISTS ${db_name};" 2>/dev/null || true
    docker exec "$container" psql -U postgres -c "CREATE DATABASE ${db_name};" 2>/dev/null

    printf "${GREEN}✓ ${db_name} reset${NC}\n"
}

remove_container() {
    local container
    container=$(find_postgres_container)

    if [[ -z "$container" ]]; then
        printf "${YELLOW}No PostgreSQL container found${NC}\n"
        return 0
    fi

    printf "${RED}Removing container: ${container}${NC}\n"
    docker rm -f "$container" 2>/dev/null || true

    # Find and remove associated volume
    local volume
    volume=$(docker volume ls --format '{{.Name}}' | grep -E "postgres" | head -1 || true)

    if [[ -n "$volume" ]]; then
        printf "${RED}Removing volume: ${volume}${NC}\n"
        docker volume rm "$volume" 2>/dev/null || true
    fi

    printf "${GREEN}✓ Container and volume removed${NC}\n"
    printf "${BLUE}Run 'dotnet run --project src/AppHost' to recreate with fresh seed data${NC}\n"
}

main() {
    local remove_container_flag=false
    local selected_dbs=()

    # Parse arguments
    while [[ $# -gt 0 ]]; do
        case "$1" in
            -h|--help)
                print_usage
                exit 0
                ;;
            -c|--container)
                remove_container_flag=true
                shift
                ;;
            -a|--all)
                selected_dbs=("productdb" "orderdb" "notificationdb")
                shift
                ;;
            product)
                selected_dbs+=("productdb")
                shift
                ;;
            order)
                selected_dbs+=("orderdb")
                shift
                ;;
            notification)
                selected_dbs+=("notificationdb")
                shift
                ;;
            *)
                printf "${RED}Unknown option: $1${NC}\n"
                print_usage
                exit 1
                ;;
        esac
    done

    printf "${BLUE}Database Reset Tool${NC}\n"
    printf "====================\n\n"

    # If removing container, do that and exit
    if [[ "$remove_container_flag" == true ]]; then
        remove_container
        exit 0
    fi

    # Default to all databases if none specified
    if [[ ${#selected_dbs[@]} -eq 0 ]]; then
        selected_dbs=("productdb" "orderdb" "notificationdb")
    fi

    # Find container
    local container
    container=$(find_postgres_container)

    if [[ -z "$container" ]]; then
        printf "${RED}Error: No PostgreSQL container found${NC}\n"
        printf "${YELLOW}Make sure Aspire is running: dotnet run --project src/AppHost${NC}\n"
        exit 1
    fi

    printf "${GREEN}Found container: ${container}${NC}\n\n"

    # Reset selected databases
    for db in "${selected_dbs[@]}"; do
        reset_database "$db" "$container"
    done

    printf "\n${GREEN}✓ Done!${NC}\n"
    printf "${BLUE}Restart the affected services to apply migrations and seed data${NC}\n"
}

main "$@"
