#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Colors
BLUE='\033[0;34m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m'

printf "${BLUE}Project Dependencies Report${NC}\n"
printf "==============================\n\n"

# Find all .csproj files and process them
find "$REPO_ROOT/src" -name "*.csproj" -type f | sort | while IFS= read -r csproj; do
    project_name=$(basename "$csproj" .csproj)
    rel_path="${csproj#$REPO_ROOT/}"

    printf "${GREEN}%s${NC}\n" "$project_name"
    printf "  ${YELLOW}Path:${NC} %s\n" "$rel_path"

    # Extract ProjectReferences (get just project name, not full path)
    project_refs=$(grep -oE '<ProjectReference Include="[^"]*"' "$csproj" 2>/dev/null \
        | sed 's/<ProjectReference Include="//;s/"//' \
        | sed 's/.*[\/\\]//;s/\.csproj$//' \
        || true)

    if [[ -n "$project_refs" ]]; then
        printf "  ${YELLOW}Projects:${NC}\n"
        echo "$project_refs" | while read -r ref; do
            printf "    - %s\n" "$ref"
        done
    fi

    # Extract PackageReferences
    package_refs=$(grep -oE '<PackageReference Include="[^"]*"' "$csproj" 2>/dev/null \
        | sed 's/<PackageReference Include="//;s/"//' \
        || true)

    if [[ -n "$package_refs" ]]; then
        printf "  ${YELLOW}Packages:${NC}\n"
        echo "$package_refs" | while read -r pkg; do
            printf "    - %s\n" "$pkg"
        done
    fi

    printf "\n"
done
