#!/bin/bash
set -uo pipefail

COMMAND=$(jq -r '.tool_input.command // empty' 2>/dev/null || true)
[ -n "${COMMAND:-}" ] || exit 0

refuse() {
  {
    printf 'Refused by this repository: %s\n' "$1"
    printf 'It keeps one branch, main. Commit where you are and push with:\n'
    printf '  git push -u origin HEAD:main\n'
  } >&2
  exit 2
}

segment_after() {
  printf '%s' "$COMMAND" | grep -oE "\\bgit\\b[^|;&]*\\b$1\\b[^|;&]*" || true
}

if printf '%s' "$COMMAND" | grep -Eq '\bgit\b[^|;&]*\b(checkout|switch)\b[^|;&]*(-b|-B|-c|-C|--create|--orphan)([[:space:]]|$)'; then
  refuse "that command starts a new branch"
fi

if printf '%s' "$COMMAND" | grep -Eq '\bgit\b[^|;&]*\bworktree[[:space:]]+add\b'; then
  refuse "git worktree add starts a new branch"
fi

BRANCH_SEGMENT=$(segment_after branch)
if [ -n "$BRANCH_SEGMENT" ]; then
  NAMES=0
  READS=0
  for token in ${BRANCH_SEGMENT#*branch}; do
    case "$token" in
      -d|-D|--delete|-r|--remotes|-a|--all|-l|--list|--show-current|--contains|--no-contains|--merged|--no-merged|--points-at|--sort=*|--format=*|-v|-vv|--verbose|-q|--quiet|--color|--no-color|-i|--ignore-case) READS=1 ;;
      -*) ;;
      *) NAMES=1 ;;
    esac
  done
  if [ "$NAMES" = 1 ] && [ "$READS" = 0 ]; then
    refuse "git branch with a name starts a new branch"
  fi
fi

PUSH_SEGMENT=$(segment_after push)
if [ -n "$PUSH_SEGMENT" ]; then
  DELETING=0
  TAGS=0
  REMOTE=0
  TOMAIN=0
  WRONG=""
  for token in ${PUSH_SEGMENT#*push}; do
    case "$token" in
      --delete|-d) DELETING=1 ;;
      --tags|--follow-tags) TAGS=1 ;;
      -*) ;;
      *)
        if [ "$REMOTE" = 0 ]; then
          REMOTE=1
        else
          DEST=${token##*:}
          case "$DEST" in
            main|refs/heads/main) TOMAIN=1 ;;
            *) WRONG=$DEST ;;
          esac
        fi ;;
    esac
  done
  if [ "$DELETING" = 0 ]; then
    if [ -n "$WRONG" ]; then
      refuse "that push would create the remote branch $WRONG"
    fi
    if [ "$TOMAIN" = 0 ] && [ "$TAGS" = 0 ]; then
      refuse "a push has to name where it goes, and main is the only place it may go"
    fi
  fi
fi

exit 0
