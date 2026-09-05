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

BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || true)

if [ -n "${BRANCH:-}" ] && [ "$BRANCH" != "main" ] && [ "$BRANCH" != "HEAD" ]; then
  if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "the harness handed over branch $BRANCH and this checkout has uncommitted work, so it was left where it is; commit here and push with: git push -u origin HEAD:main"
  elif git checkout main >/dev/null 2>&1 || git checkout -b main origin/main >/dev/null 2>&1; then
    git merge --ff-only origin/main >/dev/null 2>&1 || true
    echo "the harness handed over branch $BRANCH; this repository keeps one branch, so the checkout was moved to main and $BRANCH left exactly as it was"
  else
    echo "the harness handed over branch $BRANCH and the checkout could not be moved to main, so commit here and push with: git push -u origin HEAD:main"
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
