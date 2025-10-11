#!/usr/bin/env zsh

set -euo pipefail

SCRIPT_DIR="${0:a:h}"
cd "$SCRIPT_DIR"

print -- "[INFO] Signing is now handled by build.sh. Delegating with SKIP_BUILD=1 SKIP_NOTARIZE=1."

SKIP_BUILD=1 SKIP_NOTARIZE=1 ./build.sh "$@"