# START HERE - EFatoraJo (.NET)

Read this first before touching docs or code.

1) Core rules: `docs-rules.md`, `app-patterns.md`, `data-contracts.md`, `security-rules.md`, `testing-rules.md`.
2) Think first: plan the change, set an anchor, and keep edits small. Align with the user before broad changes.
3) Docs placement: update `README.md`/`REFERENCE.md` when needed; put new guides under `ShamDevs.EFatoraJo.Sdk/docs/` or `EFatoraJoConsoleApp/README.md`/`docs`. No new markdown in repo root without approval.
4) Naming/version: use kebab-case for new docs; keep versions/badges in sync with `.csproj`/NuGet.
5) Language: English only; save as UTF-8, avoid mojibake/emoji.
6) SDK vs Console: always state whether the change targets the SDK (netstandard2.0+) or the CLI (net8.0) and how it will be used.
