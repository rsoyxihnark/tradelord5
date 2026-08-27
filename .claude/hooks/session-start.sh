#!/bin/bash
set -euo pipefail

cd "${CLAUDE_PROJECT_DIR:-$PWD}"

ROOT_COMMIT=$(git rev-list --max-parents=0 HEAD 2>/dev/null | tail -1 || true)
if [ -n "${ROOT_COMMIT:-}" ]; then
  OWNER_NAME=$(git log -1 --format=%an "$ROOT_COMMIT")
  OWNER_ADDRESS=$(git log -1 --format=%ae "$ROOT_COMMIT")
  if [ -n "$OWNER_NAME" ] && [ "${OWNER_ADDRESS##*@}" != "anthropic.com" ]; then
    git config --local user.name "$OWNER_NAME"
    git config --local user.email "$OWNER_ADDRESS"
    git config --local commit.gpgsign false
    echo "commits are authored as $OWNER_NAME, signing off"
  else
    echo "could not read the owner from the first commit - set user.name and user.email before committing"
  fi
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
