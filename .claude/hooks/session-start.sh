#!/bin/bash
set -euo pipefail

cd "${CLAUDE_PROJECT_DIR:-$PWD}"

SIGNATURE=$(sed -n 's/.*commit as `\([^`]*\)`.*/\1/p' CLAUDE.md | head -1)
OWNER_NAME=${SIGNATURE%% <*}
OWNER_ADDRESS=${SIGNATURE#*<}
OWNER_ADDRESS=${OWNER_ADDRESS%>}
OWNER_SOURCE="the working rules, which carry the one signature this repository commits under"

if [ "$OWNER_NAME" = "$SIGNATURE" ] || [ "$OWNER_ADDRESS" = "$SIGNATURE" ]; then
  OWNER_NAME=""
  OWNER_ADDRESS=""
fi

if [ -n "${OWNER_NAME:-}" ] && [ -n "${OWNER_ADDRESS:-}" ]; then
  git config --local user.name "$OWNER_NAME"
  git config --local user.email "$OWNER_ADDRESS"
  git config --local commit.gpgsign false
  echo "commits are authored as $OWNER_NAME, read from $OWNER_SOURCE, signing off"
else
  echo "could not work out who the owner is, so set user.name and user.email before committing"
fi

git fetch --quiet origin main >/dev/null 2>&1 || true

LATEST=$(git rev-parse --verify --quiet FETCH_HEAD 2>/dev/null || true)
[ -n "${LATEST:-}" ] || LATEST=$(git rev-parse --verify --quiet refs/remotes/origin/main 2>/dev/null || true)

BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || true)
WAS=$(git rev-parse --short HEAD 2>/dev/null || true)
HERE=$(git rev-parse --verify --quiet HEAD 2>/dev/null || true)

if [ -z "${LATEST:-}" ]; then
  echo "origin/main could not be read, so this checkout was left on ${BRANCH:-HEAD} at ${WAS:-an unknown commit}; fetch it and read HEAD against origin/main before changing anything"
elif ! git diff --quiet || ! git diff --cached --quiet; then
  echo "this checkout has uncommitted work, so it was left exactly as it was, on ${BRANCH:-HEAD} at ${WAS:-an unknown commit}; origin/main is $(git rev-parse --short "$LATEST") and is the source to work from, so say what the uncommitted work is before anything discards it"
elif [ "${HERE:-}" = "$LATEST" ] && [ "$BRANCH" = "main" ]; then
  echo "the checkout is on main at origin/main, $WAS, which is the source to work from"
elif { git checkout main >/dev/null 2>&1 || git checkout -B main "$LATEST" >/dev/null 2>&1; } && git reset --hard "$LATEST" >/dev/null 2>&1; then
  echo "the checkout was on ${BRANCH:-HEAD} at $WAS and has been put on main at origin/main, $(git rev-parse --short HEAD), which is the source to work from; $WAS is still reachable through git reflog, and ${BRANCH:-HEAD} was left exactly as it was"
else
  echo "the checkout could not be put on origin/main, so it is still on ${BRANCH:-HEAD} at ${WAS:-an unknown commit}; bring it up to date yourself before changing anything"
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
