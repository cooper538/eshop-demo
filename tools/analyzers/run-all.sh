#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo "========================================"
echo "Running all analyzers..."
echo "========================================"

EXIT_CODE=0

echo ""
echo ">>> Unused Packages"
echo "----------------------------------------"
if ! "$SCRIPT_DIR/unused-packages/analyze.sh"; then
    EXIT_CODE=1
fi

echo ""
echo ">>> Code Quality"
echo "----------------------------------------"
if ! "$SCRIPT_DIR/code-quality/analyze.sh"; then
    EXIT_CODE=1
fi

echo ""
echo ">>> Security"
echo "----------------------------------------"
if ! "$SCRIPT_DIR/security/analyze.sh"; then
    EXIT_CODE=1
fi

echo ""
echo "========================================"
if [ $EXIT_CODE -eq 0 ]; then
    echo "All analyzers passed!"
else
    echo "Some analyzers reported issues."
fi
echo "========================================"

exit $EXIT_CODE
