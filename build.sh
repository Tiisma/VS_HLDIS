#!/usr/bin/env bash
# Builds HLDIS in Release and writes the ready-to-install mod ZIP to dist/.
# The ZIP content (dll, pdb, modinfo.json, modicon.png, assets/) is defined by the PackageMod target in HLDIS/HLDIS.csproj.
#
# Usage: ./build.sh [path/to/vintagestory] [--test]
#   path/to/vintagestory  Game folder holding VintagestoryAPI.dll (default: $VINTAGE_STORY).
#   --test                Also run the Atlas test suite before packaging.
set -euo pipefail

usage() {
    sed -n '2,7p' "$0" | sed 's/^# \{0,1\}//'
    exit "${1:-0}"
}

fail() {
    echo "error: $*" >&2
    exit 1
}

GAME_PATH=""
RUN_TESTS=0
for arg in "$@"; do
    case "$arg" in
        --test) RUN_TESTS=1 ;;
        -h|--help) usage 0 ;;
        -*) echo "error: unknown option '$arg'" >&2; usage 1 ;;
        *) GAME_PATH="$arg" ;;
    esac
done

GAME_PATH="${GAME_PATH:-${VINTAGE_STORY:-}}"
[ -n "$GAME_PATH" ] || fail "no game folder: pass it as an argument or set VINTAGE_STORY."
[ -f "$GAME_PATH/VintagestoryAPI.dll" ] || fail "VintagestoryAPI.dll not found in '$GAME_PATH'."
command -v dotnet >/dev/null 2>&1 || fail "dotnet not found: install the .NET 10 SDK."

ROOT="$(dirname "$0")"

if [ "$RUN_TESTS" -eq 1 ]; then
    echo "==> Running tests against '$GAME_PATH'"
    VINTAGE_STORY="$GAME_PATH" dotnet test "$ROOT/HLDIS.Tests/HLDIS.Tests.csproj" -nologo
fi

echo "==> Building mod against '$GAME_PATH'"
rm -rf "$ROOT/dist" || fail "cannot clean dist/: close any program using the ZIP and retry."
dotnet build "$ROOT/HLDIS/HLDIS.csproj" -c Release -nologo -p:VintageStoryPath="$GAME_PATH"

ZIP="$(ls "$ROOT"/dist/hldis_*.zip 2>/dev/null | head -n 1)"
[ -n "$ZIP" ] || fail "build finished but no ZIP was written to dist/."
echo "==> Ready: $ZIP"
