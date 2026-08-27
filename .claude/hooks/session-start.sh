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
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

if [ ! -x "$DOTNET_DIR/dotnet" ]; then
  curl -sSL --retry 3 --max-time 600 -o /tmp/dotnet-install.sh https://dot.net/v1/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 8.0 --install-dir "$DOTNET_DIR" --no-path
  rm -f /tmp/dotnet-install.sh
fi

export DOTNET_ROOT="$DOTNET_DIR"
export PATH="$DOTNET_DIR:$PATH"

if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  {
    echo "export DOTNET_ROOT=\"$DOTNET_DIR\""
    echo "export PATH=\"$DOTNET_DIR:\$PATH\""
    echo "export DOTNET_CLI_TELEMETRY_OPTOUT=1"
    echo "export DOTNET_NOLOGO=1"
  } >> "$CLAUDE_ENV_FILE"
fi

if dotnet restore src/TradeLord.csproj >/dev/null 2>&1 &&
   dotnet restore mcm/TradeLord.MCM.csproj >/dev/null 2>&1; then
  echo "dotnet $(dotnet --version) ready, packages restored - build both csproj before pushing"
else
  echo "dotnet $(dotnet --version) ready, but package restore failed - run dotnet restore and check the proxy"
fi
