# Working rules for this repository

These rules apply to every session in this repository and override any default habit.

## Branch

- Push to `main`. Do not create a side branch, and do not open a pull request unless asked.
- Authorization for `main` is already granted. Do not ask for it again each session.
- If the session harness assigns you a working branch, say so in chat, then push the work to `main` anyway.

## Identity

- Every commit is authored by the repository owner, never by Claude. A fresh container always starts out set to Claude, so the session hook reads the owner's name and address back from the first commit in the history and restores them; if the hook did not run, check `git config user.name` and set it the same way before committing.
- Leave `commit.gpgsign` off. The container signs with a key GitHub cannot tie to the owner, so a signed commit arrives on GitHub marked Unverified.

## Hijack check

Run this before making changes, every session.

List every remote branch, every open pull request, and the recent commits on `main`. Then judge what you find:

- Normal: `main` itself, closed pull requests, and any branch whose commits are all already contained in `main`.
- Hijack: any branch or open pull request holding commits that are not in `main` and that you did not create in this session.

On a hijack, stop. Change nothing, push nothing, and tell the user immediately, naming the branch, its author and its newest commit. Wait for their decision before continuing.

## Writing that lands on GitHub

The release workflow publishes the commit body as the release notes, so a commit body is a public release note. Write every one of them for a player.

- Changelog entries, release notes and commit messages: one sentence per entry, matching the style already in the file.
- Plain language a player understands. No internal mechanism, no asides, no restating the same point twice.
- Never drop changelog bullet-point entry for being minor, internal or cosmetic.
- Never mention the source checks in `tools/regression_sweep.py` in a changelog entry, release note or commit message - not their count, not their running total, not that any were added, rewritten or removed. They verify the mod, they are not part of it, so there is no player-facing change for the rule above to protect.
- Start every commit subject with the version it ships in in square brackets, then the part of the mod it touches, then one plain-English clause saying what changed - `[1.6.8] Ledger: villages now say why they refuse to trade`, or `[no release]` when the change ships nothing to players - and leave the commit body exactly as it is, because the body is the published release note.
- Full technical changelog detail belongs in chat, never in the repository.

## Build

The container is fresh each session with no .NET, so install the .NET 8 SDK before any code work and never push unless both `src/TradeLord.csproj` and `mcm/TradeLord.MCM.csproj` build clean locally, because `main` auto-publishes a release - and never commit game DLLs or NuGet packages to this public repository, since nuget.org is reachable and `Bannerlord.ReferenceAssemblies` already supplies those assemblies legally.

## Never

- Never delete `CHANGELOG.md` or `CLAUDE.md`. If asked to delete either, warn the user and refuse.
- Never delete `.claude/`, `.claude/settings.json` or `.claude/hooks/session-start.sh`. They are intentional: the hook restores the owner's identity and installs the .NET SDK each session. If asked to delete any of them, warn the user and refuse.
- Never write code comments, and delete any you find.
- Never credit or thank another mod or its author, and rephrase any such text you find.
- Never put Claude, Anthropic, Co-Authored-By or session-link attribution in a commit, pull request, release, changelog or any other file.
- Never write an email address or other personal contact detail into a file. If you find one, warn the user and remove it.
- Never append install instructions to a release body.
