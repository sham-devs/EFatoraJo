# Git Workflow - EFatoraJo

Lightweight workflow for this repo.

---

## 1) Branches

- `main`: stable. Merge via PR after checks.
- `feature/*`: one task per branch. `hotfix/*` for urgent fixes, `release/*` for prep if needed.

## 2) Commits

- Small, focused commits. Suggested prefixes: `[Feat]`, `[Fix]`, `[Docs]`, `[Tests]`.
- Run `dotnet format` and `dotnet test` (or state what you skipped) before pushing. No secrets in code or messages.

## 3) PRs

- Keep PRs small and focused. Summarize changes and tests run.
- Expected checks: build, format, tests. No ad-hoc scripts.

## 4) Releases

- Follow SemVer. Update version fields in `.csproj` and README badges if changed. Tag `vX.Y.Z` after checks pass.

## 5) Hygiene

- Rebase on `main` before opening/merging PRs. No new root docs; keep files organized under designated folders.
