#!/bin/bash
set -uof pipefail

refuse() {
  {
    printf 'Refused by this repository: %s\n' "$1"
    printf 'It keeps one branch, main. Commit where you are and push with:\n'
    printf '  git push -u origin HEAD:main\n'
    printf 'If that line is text, such as a heredoc or a script body, rather than a command, write it with a file tool instead.\n'
  } >&2
  exit 2
}

flagged() {
  local rest=${1#-} one
  while [ -n "$rest" ]; do
    one=${rest:0:1}
    rest=${rest:1}
    case "$one" in
      [$2]) return 0 ;;
      [$3]) return 1 ;;
    esac
  done
  return 1
}

INPUT=$(cat)

TOOL=""
NAMED='"tool_name"[[:space:]]*:[[:space:]]*"([^"]*)"'
[[ $INPUT =~ $NAMED ]] && TOOL=${BASH_REMATCH[1]}
case "$TOOL" in
  mcp__*)
    case "$TOOL" in
      *__create_branch)
        refuse "that tool starts a new branch on GitHub" ;;
      *__create_pull_request)
        refuse "a pull request needs a second branch, and main is the only branch" ;;
      *__update_pull_request_branch)
        refuse "that tool writes to the branch of a pull request, and main is the only branch" ;;
      *__push_files|*__create_or_update_file|*__delete_file)
        TARGET=""
        BRANCH='"branch"[[:space:]]*:[[:space:]]*"([^"]*)"'
        [[ $INPUT =~ $BRANCH ]] && TARGET=${BASH_REMATCH[1]}
        case "$TARGET" in
          ''|main|refs/heads/main) ;;
          *) refuse "that tool writes to the branch $TARGET on GitHub, and main is the only branch" ;;
        esac ;;
    esac
    exit 0 ;;
esac

if command -v jq >/dev/null 2>&1; then
  COMMAND=$(printf '%s' "$INPUT" | jq -r '.tool_input.command // empty' 2>/dev/null || true)
elif python3 -c 'import json' >/dev/null 2>&1; then
  COMMAND=$(printf '%s' "$INPUT" | python3 -c 'import json, sys; print((json.load(sys.stdin).get("tool_input") or {}).get("command") or "")' 2>/dev/null || true)
else
  COMMAND=$(printf '%s' "$INPUT" | sed -n 's/.*"command"[[:space:]]*:[[:space:]]*"\(\([^"\\]\|\\.\)*\)".*/\1/p' |
            sed -e 's/\\n/\n/g' -e 's/\\t/\t/g' -e 's/\\"/"/g' -e 's/\\\\/\\/g')
  if [ -z "$COMMAND" ]; then
    case "$INPUT" in
      *git*)
        printf 'Refused by this repository: the branch guard needs jq or python3 to read a git command, and has neither\n' >&2
        exit 2 ;;
    esac
    exit 0
  fi
fi
[ -n "${COMMAND:-}" ] || exit 0

SEGMENTS=$(printf '%s\n' "$COMMAND" | { sed 's/%([^)]*)/%/g' 2>/dev/null || cat; } | tr '|;&()`' '\n\n\n\n\n\n')

while IFS= read -r segment; do
  set -- ${segment//[\'\"]/}
  for token in "$@"; do
    case "$token" in
      GIT_CONFIG_KEY_*=[Aa][Ll][Ii][Aa][Ss].*|GIT_CONFIG_PARAMETERS=*[Aa][Ll][Ii][Aa][Ss].*)
        refuse "that command defines a git alias through the environment, and the guard cannot see into an alias" ;;
    esac
  done
  FED=0
  while [ $# -gt 0 ]; do
    case "$1" in
      timeout) shift
        while [ $# -gt 0 ]; do
          case "$1" in
            -s|-k|--signal|--kill-after) shift; shift ;;
            -*) shift ;;
            *) shift; break ;;
          esac
        done ;;
      nice) shift
        case "${1:-}" in -n|--adjustment) shift; shift ;; esac ;;
      sudo) shift
        while [ $# -gt 0 ]; do
          case "$1" in
            -u|-g|-p|-a|-C|-D|-h|-r|-t|-U|-T|--user|--group|--prompt|--chdir|--host|--role|--type) shift; shift ;;
            --other-user|--command-timeout|--close-from) shift; shift ;;
            -*) shift ;;
            *) break ;;
          esac
        done ;;
      exec) shift
        case "${1:-}" in -a) shift; shift ;; esac ;;
      xargs) shift; FED=1
        while [ $# -gt 0 ]; do
          case "$1" in
            -n|-I|-L|-P|-s|-d|-E|-a|--max-args|--max-lines|--max-procs|--delimiter|--arg-file|--process-slot-var) shift; shift ;;
            -*) shift ;;
            *) break ;;
          esac
        done ;;
      bash|sh|zsh|dash|ksh|*/bash|*/sh|*/zsh|*/dash|*/ksh) shift
        while [ $# -gt 0 ]; do
          case "$1" in
            -o|-O|+o|+O|--rcfile|--init-file) shift; shift ;;
            -c*|-[!-]*c*) shift; break ;;
            -*|+*) shift ;;
            *) set --; break ;;
          esac
        done ;;
      find|*/find) shift
        while [ $# -gt 0 ]; do
          case "$1" in
            -exec|-execdir|-ok|-okdir) shift; break ;;
            *) shift ;;
          esac
        done ;;
      -u|-g|-C|--unset|--chdir) shift; shift ;;
      builtin|eval) shift ;;
      env|command|exec|nohup|time|sudo|then|do|else|elif|if|while|until|'!'|'{'|-*) shift ;;
      [A-Za-z_]*=*) shift ;;
      *) break ;;
    esac
  done
  [ "$FED" = 1 ] && set -- "$@" "{}"
  case "${1:-}" in
    gh|*/gh|curl|*/curl)
      if [ "${1##*/}" = gh ]; then
        STEP=0; SKIP=0
        for token in "$@"; do
          if [ "$SKIP" = 1 ]; then SKIP=0; continue; fi
          case "$token" in
            -R|--repo|--hostname) SKIP=1; continue ;;
            -*) continue ;;
          esac
          case "$STEP:$token" in
            0:*) STEP=1 ;;
            1:pr) STEP=2 ;;
            1:alias) STEP=3 ;;
            2:create|2:checkout) refuse "gh pr $token needs or makes a second branch, and main is the only branch" ;;
            3:set|3:import) refuse "that command defines a gh alias, and the guard cannot see into an alias" ;;
            *) break ;;
          esac
        done
      fi
      REFS=0; WRITE=0; DELETE=0; MAIN=0; TAGGED=0; OTHER=""
      for token in "$@"; do
        case "$token" in
          *createRef*|*updateRefs*|*[Cc]reate*[Bb]ranch*)
            refuse "that request starts a new branch on GitHub" ;;
          *branches/*/rename*)
            refuse "that request renames a branch on GitHub, and main is the only branch" ;;
        esac
        case "$token" in --expand-*) token=--${token#--expand-} ;; esac
        case "$token" in *git/refs*) REFS=1 ;; esac
        case "$token" in *refs/tags/*) TAGGED=1 ;; esac
        case "$token" in
          POST|post|PATCH|patch|PUT|put|-*XPOST|-*XPATCH|-*XPUT|*=POST|*=PATCH|*=PUT) WRITE=1 ;;
          DELETE|delete|-*XDELETE|*=DELETE) DELETE=1 ;;
          --field*|--raw-field*|--input*|--data*|--json*|--form*|--upload-file*) WRITE=1 ;;
          --*) ;;
          -*)
            if [ "${1##*/}" = gh ]; then
              flagged "$token" fF HpqtX && WRITE=1
            else
              flagged "$token" dFT AbcCDeEHKmoPQrtuUwxXyYz && WRITE=1
            fi ;;
        esac
        rest=$token
        while :; do
          case "$rest" in
            *refs/heads/*) rest=${rest#*refs/heads/} ;;
            *) break ;;
          esac
          name=${rest%%[!A-Za-z0-9._/-]*}
          case "${rest#"$name"}" in
            ''|[],}?\&#\\:]*) ;;
            *) name="$name${rest#"$name"}" ;;
          esac
          if [ "$name" = main ]; then MAIN=1; else OTHER=${OTHER:-$name}; fi
        done
      done
      if [ "$REFS" = 1 ]; then
        [ "$DELETE" = 1 ] && [ "$MAIN" = 1 ] && refuse "that request would delete main on GitHub"
        [ "$DELETE" = 0 ] && [ "$WRITE" = 1 ] && [ -n "$OTHER" ] &&
          refuse "that request starts or moves the branch $OTHER on GitHub, and main is the only branch"
        [ "$MAIN" = 0 ] && [ "$TAGGED" = 0 ] && [ -z "$OTHER" ] && { [ "$WRITE" = 1 ] || [ "$DELETE" = 1 ]; } &&
          refuse "that request changes a ref on GitHub, and the guard cannot read which one"
      fi
      continue ;;
  esac
  case "${1:-}" in
    git|*/git) ;;
    git.exe|*/git.exe) ;;
    *) continue ;;
  esac
  shift
  while [ $# -gt 0 ]; do
    case "$1" in
      --super-prefix|--attr-source) shift; shift ;;
      -c|--config-env)
        case "${2:-}" in
          [Aa][Ll][Ii][Aa][Ss].*) refuse "that command defines a git alias inline, and the guard cannot see into an alias" ;;
        esac
        shift; shift ;;
      --config-env=[Aa][Ll][Ii][Aa][Ss].*)
        refuse "that command defines a git alias inline, and the guard cannot see into an alias" ;;
      -C|-c|--git-dir|--work-tree|--namespace|--config-env) shift; shift ;;
      -*) shift ;;
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
          --cr*|--force-c*|--or*|--tr*)
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
      if [ -n "$FIRST" ] && [ "$FIRST" != main ] && [ "$FIRST" != HEAD ] && [ "$DETACH" = 0 ] && [ "$GUESS" = 1 ] &&
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
          --stdin) refuse "git update-ref --stdin can start a branch the guard cannot read" ;;
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

    symbolic-ref)
      for token in "$@"; do
        case "$token" in
          -d|--delete) break ;;
          refs/heads/main) ;;
          refs/heads/*) refuse "that command points HEAD at a new branch" ;;
        esac
      done ;;

    config)
      READING=0; ALIAS=0
      for token in "$@"; do
        case "$token" in
          --get|--get-all|--get-regexp|--get-urlmatch|-l|--list|--unset|--unset-all|--remove-section) READING=1 ;;
          get|list|unset|remove-section) READING=1 ;;
          [Aa][Ll][Ii][Aa][Ss]|[Aa][Ll][Ii][Aa][Ss].*) ALIAS=1 ;;
        esac
      done
      [ "$ALIAS" = 1 ] && [ "$READING" = 0 ] &&
        refuse "that command defines a git alias, and the guard cannot see into an alias" ;;

    send-pack|http-push|fast-import)
      refuse "git $SUB can start a branch the guard cannot read" ;;

    subtree)
      PUSHING=0; LAST=""
      for token in "$@"; do
        case "$token" in
          -b*|--b*) refuse "git subtree with a branch starts a new branch" ;;
          push) PUSHING=1 ;;
        esac
        LAST=$token
      done
      if [ "$PUSHING" = 1 ]; then
        case "${LAST##*:}" in
          main|refs/heads/main) ;;
          *) refuse "that subtree push would create the remote branch ${LAST##*:}" ;;
        esac
      fi ;;

    fetch|pull)
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
          -d|-D|--delete|-r|--remotes|-a|--all|-l|--list|--show-current|--edit-description) READS=1 ;;
          --contains|--contains=*|--no-contains|--no-contains=*|--merged|--merged=*) READS=1 ;;
          --no-merged|--no-merged=*|--points-at|--points-at=*) READS=1 ;;
          -u|--set-upstream-to|--set-upstream-to=*|--unset-upstream) READS=1 ;;
          --format|--sort) SKIP=1 ;;
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
          --de*|-[!o-]*d*) DELETING=1 ;;
          --tags|--follow-tags) TAGS=1 ;;
          -o|--push-option|--repo|--receive-pack|--exec) SKIP=1 ;;
          -*) ;;
          *)
            if [ "$REMOTE" = 0 ]; then
              REMOTE=1
            else
              case "$token" in
                :*|+:*)
                  case "${token##*:}" in
                    main|refs/heads/main) refuse "that push would delete main from the remote" ;;
                  esac ;;
              esac
              case "${token##*:}" in
                main|refs/heads/main) TOMAIN=1 ;;
                *) WRONG=${token##*:} ;;
              esac
            fi ;;
        esac
      done
      [ "$DELETING" = 1 ] && [ "$TOMAIN" = 1 ] &&
        refuse "that push would delete main from the remote"
      if [ "$DELETING" = 0 ]; then
        [ -n "$WRONG" ] && refuse "that push would create the remote branch $WRONG"
        [ "$TOMAIN" = 0 ] && [ "$TAGS" = 0 ] &&
          refuse "a push has to name where it goes, and main is the only place it may go"
      fi ;;
  esac
done <<<"$SEGMENTS"

exit 0
