#!/bin/bash
set -euo pipefail

cd "${CLAUDE_PROJECT_DIR:-$PWD}"

OWNER_LINE=$(git log --format='%an%x1f%ae' 2>/dev/null | awk -F'\037' '
  {
    name = tolower($1)
    host = tolower($2)
    sub(/.*@/, "", host)
  }
  name ~ /claude/ { next }
  host == "anthropic.com" { next }
  { print; exit }
' || true)

if [ -n "${OWNER_LINE:-}" ]; then
  OWNER_NAME=${OWNER_LINE%%$'\037'*}
  OWNER_ADDRESS=${OWNER_LINE##*$'\037'}
  OWNER_SOURCE="the most recent commit the owner authored"
else
  FALLBACK=$(sed -n 's/.*commit as `\([^`]*\)`.*/\1/p' CLAUDE.md | head -1)
  OWNER_NAME=${FALLBACK%% <*}
  OWNER_ADDRESS=${FALLBACK#*<}
  OWNER_ADDRESS=${OWNER_ADDRESS%>}
  OWNER_SOURCE="the working rules, since no commit in the history was authored by the owner"
fi

if [ -n "${OWNER_NAME:-}" ] && [ -n "${OWNER_ADDRESS:-}" ]; then
  git config --local user.name "$OWNER_NAME"
  git config --local user.email "$OWNER_ADDRESS"
  git config --local commit.gpgsign false
  echo "commits are authored as $OWNER_NAME, read from $OWNER_SOURCE, signing off"
else
  echo "could not work out who the owner is, so set user.name and user.email before committing"
fi

if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

DOTNET_DIR="$HOME/.dotnet"

if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  {
    echo "export DOTNET_ROOT=\"$DOTNET_DIR\""
    echo "export PATH=\"$DOTNET_DIR:\$PATH\""
    echo "export DOTNET_CLI_TELEMETRY_OPTOUT=1"
    echo "export DOTNET_NOLOGO=1"
  } >> "$CLAUDE_ENV_FILE"
fi

if [ -x "$DOTNET_DIR/dotnet" ]; then
  echo "dotnet $("$DOTNET_DIR/dotnet" --version) ready - build both csproj before pushing"
else
  echo "no .NET here yet - both csproj must still build clean before any push, so install it when code work starts:"
  echo "  curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir \"\$HOME/.dotnet\" --no-path"
fi
