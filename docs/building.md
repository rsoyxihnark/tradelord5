# Building from source

```
dotnet build src/TradeLord.csproj -c Release
dotnet build mcm/TradeLord.MCM.csproj -c Release
```

Game assemblies come from the `Bannerlord.ReferenceAssemblies` NuGet package, so no game install is
needed to build and no game DLLs live in this repository. Both projects build with warnings as
errors.

## What is where

| Path | What it holds |
|---|---|
| `src/` | The mod: ledger, trading, routes, tooltips, the map panel. |
| `mcm/` | The settings screen, built separately so the mod runs without MCM. |
| `TradeLord/` | The module folder that ships: manifest, GUI prefabs, language strings. |
| `tests/` | Unit tests for the save codec, route rules and trade maths. |
| `tools/` | The source checks and the game-version compatibility tool. |

## Tests and checks

```
dotnet test tests/TradeLord.Tests.csproj -c Release
python3 tools/regression_sweep.py
```

Both run in the build workflow on every push.

## Checking a game version

Build both projects in Release, then:

```
dotnet run --project tools/compat -- 1.4.7.117484 1.5.1.120547-beta
```

It fetches the reference assemblies for each version named, compares them against the version in
`src/TradeLord.csproj`, and exits non-zero if anything the mod binds to has moved. The table in
[docs/compatibility.md](compatibility.md#game-versions) is built from its output.

## Releases

Pushing to `main` builds the module, zips it and publishes a release named by the version in
`TradeLord/SubModule.xml`. The newest changelog entry becomes the release notes, so the changelog
has to open on the version the manifest declares.
