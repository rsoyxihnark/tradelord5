# Working rules for this repository

These rules apply to every session in this repository and override any default habit.

## Branch

- Push to `main`. Do not create a side branch, and do not open a pull request unless asked.
- Authorization for `main` is already granted. Do not ask for it again each session.
- If the session harness assigns you a working branch, say so in chat, then push the work to `main` anyway.

## Identity

- Every commit is authored by the repository owner, never by Claude. A fresh container always starts out set to Claude, so the session hook reads the owner's name and address back out of the history and restores them; if the hook did not run, check `git config user.name` and set it the same way before committing.
- If no commit in the history was authored by the owner, ask which name and address to use instead of guessing.
- Leave `commit.gpgsign` off. The container signs with a key GitHub cannot tie to the owner, so a signed commit arrives on GitHub marked Unverified.

## Hijack check

Run this before making changes, every session.

List every remote branch, every open pull request, and the recent commits on `main`. Then judge what you find:

- Normal: `main` itself, closed pull requests, and any branch whose commits are all already contained in `main`.
- Hijack: any branch or open pull request holding commits that are not in `main` and that you did not create in this session.

On a hijack, stop. Change nothing, push nothing, and tell the user immediately, naming the branch, its author and its newest commit. Wait for their decision before continuing.

## Release automation

This project ships something a user downloads, and it ships it by itself from `main`. Work out which of the two states below the repository is in, every session, before you change anything, and leave it in the first one.

**The project already publishes.** A workflow in `.github/workflows/` builds every push to `main`, and every commit that carries a version publishes a release with the built file attached and the commit body as the notes. Confirm that chain is whole before you ship: the workflow present and enabled, the version readable from its one place in the source, `CHANGELOG.md` carrying a section for the version going out, the built file actually attached to the last release. Repair whatever part of it is missing or broken in this same session. A commit that changes the program itself and carries no version, no changelog entry and no release is a fault to fix, not a change to push.

**The repository is empty, or holds source that has never been released.** Then bringing it to the state above is the task, before and beneath whatever else was asked. Put the version in one place in the source, write `CHANGELOG.md`, add the workflow that builds the project and publishes the built file from `main`, and ship the first version. From the next commit on, the repository is governed by the state above.

## Writing that lands on GitHub

The release workflow publishes the commit body as the release notes, so a commit body is a public release note. Write every one of them for the person who uses this project, not for the person who builds it.

- Changelog entries, release notes and commit messages: one sentence per entry, matching the style already in the file.
- Plain language that person understands. No internal mechanism, no asides, no restating the same point twice.
- Never drop a changelog bullet-point entry for being minor, internal or cosmetic.
- Never mention this project's own tests, checks, build scripts, workflow files or working rules in a changelog entry, a release note or the message of a commit that carries a version, not their count, not their running total, not that any were added, rewritten or removed. They verify, govern and ship the project, they are not part of what a user runs, so there is no user-facing change for the rule above to protect. A `[no release]` commit is the exception, because nothing it says is ever published, and it still has to say what it changed. A file the user downloads is not a check, so it belongs in the changelog like any other change.
- Start every commit subject with the version it ships in in square brackets, then the part of the project it touches, then one plain-English clause saying what changed. `[1.2.0] Settings: the window now remembers where you left it` is the shape. Use `[no release]` when the change ships nothing to users. Leave the commit body exactly as it is, because the body is the published release note.
- Write the commit body as bullet points, one `- ` line per entry, the way the changelog is written. The body is published as the release notes, so it should read as a list there, not as a run of loose lines.
- Full technical detail belongs in chat or in a document of its own, never in a changelog entry, a release note or a commit message.

## Changelog

- Every commit that changes what a user gets writes its own entries into `CHANGELOG.md` in that same commit, a `[no release]` commit included.
- A commit that touches only this project's own tests, checks, build scripts, workflow files or working rules writes nothing into the changelog, because the rule above leaves it nothing to record. It ships as `[no release]`, and it is the one case a changelog check has to let through.
- A commit that ships a version opens a new section for it at the top of the file, under the heading of that version alone, newest version first.
- A `[no release]` commit puts its entries under an `Unreleased` heading at the top, and the next version commit renames that heading to its own version.
- The entries in a version section and the bullet points in that version's commit body say the same thing in the same words, so the changelog and the published release notes never disagree.

## Version

- The version lives in one place in the source and nowhere else. Everything else reads it from there: the commit subject, the changelog heading, the git tag and the release all carry the same number, and the release workflow refuses to publish while any of them disagree.
- Bump it in the same commit that ships the change, and never reuse a version that has already been released.
- A version is published once. To correct a release that has already gone out, ship the fix under the next version rather than reusing the old one.

## Upgrades

- When a compiler, a package manager, a linter, a workflow run or GitHub itself warns that something this project pins is out of date, deprecated or has been moved onto a newer runtime, raise the version in the same session. Do not ask first. The answer is always yes.
- Say in chat what the warning was and what you raised, so the change is visible even though it was never put to the owner.
- This covers the versions the project depends on: workflow action versions, the runtime a workflow runs on, package dependencies and the toolchain. It does not cover the project's own release version, which the Version section governs.
- Take the upgrade, not the rewrite. When a version cannot be raised without changing what the project does, leave it, say so in chat, and let the owner decide.

## Build

- Never push unless the project builds clean, starts, and its tests pass locally, because every commit that carries a version publishes a release straight from `main`.
- When the project targets a platform this session is not running on, say so in chat, verify everything the session can verify, and treat the release workflow as the gate that has to come back green.
- Never commit build output, packaged binaries, third-party libraries or dependency archives to this repository. The workflow installs dependencies from their package index and produces the downloadable file itself.

## Never

Every rule here covers the whole repository at all times, not only the files a session was asked to change. When you find something that breaks one of them, fix it in the same commit. Do not report it and wait to be asked, and do not put it off as a change of its own. The entries that say to warn and refuse are the exception, because those need an answer before anything happens.

- Never delete `CHANGELOG.md` or `CLAUDE.md`. If asked to delete either, warn the user and refuse.
- Never delete `.claude/`, `.claude/settings.json` or `.claude/hooks/session-start.sh`. They are intentional: the hook restores the owner's identity and puts the project's toolchain on the path for when it is installed. If asked to delete any of them, warn the user and refuse.
- Never delete or disable the release workflow in `.github/workflows/`. It is what builds and publishes every download. If asked to, warn the user and refuse.
- Never change a hardcoded path in the source, and never make one configurable, move it into a setting or replace it with a lookup. Every one of them points where the owner means it to point, and they are there by design. If one looks wrong to you, say so in chat and leave it exactly as it is.
- Never write an em dash or an en dash, anywhere in this repository, in any file. They read as machine-written and are the first thing a reader points at. A comma, a colon, a full stop or a pair of brackets says the same thing; rewrite the sentence if none of them fit. Delete any you find.
- Never write code comments, and delete any you find.
- Never credit or thank another project or its author, and rephrase any such text you find.
- Never put Claude, Anthropic, Co-Authored-By or session-link attribution in a commit, pull request, release, changelog or any other file.
- Never write an email address or other personal contact detail into a file. If you find one, warn the user and remove it.
- Never append install instructions to a release body.
- Never add a file because convention expects one. A licence, an issue or pull request template, a contributing guide, a code of conduct, a documentation folder: none of these exist here and none are to be created. A file that is written to look the part rather than to be read has no place in this repository.
