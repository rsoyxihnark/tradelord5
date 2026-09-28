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
  while [ $# -gt 0 ]; do
    case "$1" in
      env|command|exec|nohup|time|sudo|-*) shift ;;
      [A-Za-z_]*=*) shift ;;
      *) break ;;
    esac
  done
  case "${1:-}" in
    git|*/git) ;;
    *) continue ;;
  esac
  shift
  while [ $# -gt 0 ]; do
    case "$1" in
      -C|-c|--git-dir|--work-tree|--namespace|--config-env) shift; shift ;;
      --git-dir=*|--work-tree=*|--namespace=*|--exec-path=*|--config-env=*|--exec-path|-p|--paginate|--no-pager|--bare|--literal-pathspecs|--no-replace-objects|--no-optional-locks|--glob-pathspecs|--noglob-pathspecs|--icase-pathspecs) shift ;;
      *) break ;;
    esac
  done
  SUB=${1:-}
  [ $# -gt 0 ] && shift

  case "$SUB" in
    checkout|switch)
      DETACH=0; GUESS=1; FIRST=""
      for token in "$@"; do
        case "$token" in
          --) break ;;
          -b|-B|-c|-C|--create|--force-create|--orphan)
            refuse "that command starts a new branch" ;;
          --create=*|--force-create=*|--orphan=*|-t|--track|--track=*)
            refuse "that command starts a new branch" ;;
          --detach) DETACH=1 ;;
          --no-guess) GUESS=0 ;;
          --*) ;;
          -*[bBcCt]*) refuse "that command starts a new branch" ;;
          -*d*) [ "$SUB" = switch ] && DETACH=1 ;;
          -*) ;;
          *) [ -z "$FIRST" ] && FIRST=$token ;;
        esac
      done
      if [ -n "$FIRST" ] && [ "$FIRST" != main ] && [ "$DETACH" = 0 ] && [ "$GUESS" = 1 ] &&
         ! git show-ref --verify --quiet "refs/heads/$FIRST" 2>/dev/null &&
         [ -n "$(git for-each-ref --format='%(refname)' "refs/remotes/*/$FIRST" 2>/dev/null)" ]; then
        { [ "$SUB" = checkout ] && [ -e "$FIRST" ]; } ||
          refuse "that command starts a local branch $FIRST from the remote one"
      fi ;;

    stash)
      [ "${1:-}" = "branch" ] && refuse "git stash branch starts a new branch" ;;

    update-ref)
      DELETING=0; SKIP=0
      for token in "$@"; do
        if [ "$SKIP" = 1 ]; then SKIP=0; continue; fi
        case "$token" in
          -d|--delete) DELETING=1 ;;
          -m) SKIP=1 ;;
          -*) ;;
          *)
            if [ "$DELETING" = 0 ]; then
              case "$token" in
                refs/heads/main) ;;
                refs/heads/*) refuse "that command starts a new branch" ;;
              esac
            fi
            break ;;
        esac
      done ;;

    fetch)
      REMOTE=0
      for token in "$@"; do
        case "$token" in
          -*) ;;
          *)
            if [ "$REMOTE" = 0 ]; then
              REMOTE=1
            else
              case "$token" in
                *://*) ;;
                *:*)
                  case "${token##*:}" in
                    ''|main|refs/heads/main|refs/remotes/*|refs/tags/*) ;;
                    *) refuse "that fetch would start the local branch ${token##*:}" ;;
                  esac ;;
              esac
            fi ;;
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
