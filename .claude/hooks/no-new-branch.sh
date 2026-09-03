#!/bin/bash
set -uof pipefail

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

SEGMENTS=$(printf '%s\n' "$COMMAND" | tr '|;&()' '\n\n\n\n\n')

while IFS= read -r segment; do
  set -- $segment
  [ "${1:-}" = "git" ] || continue
  shift
  while [ $# -gt 0 ]; do
    case "$1" in
      -C|-c) shift; shift ;;
      --git-dir=*|--work-tree=*|--namespace=*|--exec-path=*|-p|--paginate|--no-pager|--bare|--literal-pathspecs|--no-replace-objects) shift ;;
      *) break ;;
    esac
  done
  SUB=${1:-}
  [ $# -gt 0 ] && shift

  case "$SUB" in
    checkout|switch)
      for token in "$@"; do
        case "$token" in
          --) break ;;
          -b|-B|-c|-C|--create|--force-create|--orphan)
            refuse "that command starts a new branch" ;;
        esac
      done ;;

    worktree)
      [ "${1:-}" = "add" ] && refuse "git worktree add starts a new branch" ;;

    branch)
      NAMES=0; READS=0; SKIP=0
      for token in "$@"; do
        if [ "$SKIP" = 1 ]; then SKIP=0; continue; fi
        case "$token" in
          *'>'*|*'<'*) case "$token" in *'>'|*'<') SKIP=1 ;; esac ;;
          -d|-D|--delete|-r|--remotes|-a|--all|-l|--list|--show-current|--contains|--no-contains|--merged|--no-merged|--points-at|--sort=*|--format=*|-v|-vv|--verbose|-q|--quiet|--color|--no-color|-i|--ignore-case) READS=1 ;;
          -*) ;;
          *) NAMES=1 ;;
        esac
      done
      [ "$NAMES" = 1 ] && [ "$READS" = 0 ] &&
        refuse "git branch with a name starts a new branch" ;;

    push)
      DELETING=0; TAGS=0; REMOTE=0; TOMAIN=0; SKIP=0; WRONG=""
      for token in "$@"; do
        if [ "$SKIP" = 1 ]; then SKIP=0; continue; fi
        case "$token" in
          *'>'*|*'<'*) case "$token" in *'>'|*'<') SKIP=1 ;; esac ;;
          --delete|-d) DELETING=1 ;;
          --tags|--follow-tags) TAGS=1 ;;
          -*) ;;
          *)
            if [ "$REMOTE" = 0 ]; then
              REMOTE=1
            else
              case "${token##*:}" in
                main|refs/heads/main) TOMAIN=1 ;;
                *) WRONG=${token##*:} ;;
              esac
            fi ;;
        esac
      done
      if [ "$DELETING" = 0 ]; then
        [ -n "$WRONG" ] && refuse "that push would create the remote branch $WRONG"
        [ "$TOMAIN" = 0 ] && [ "$TAGS" = 0 ] &&
          refuse "a push has to name where it goes, and main is the only place it may go"
      fi ;;
  esac
done <<<"$SEGMENTS"

exit 0
