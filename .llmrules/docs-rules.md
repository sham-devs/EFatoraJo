# Docs Rules - EFatoraJo

How and where to write documentation for this repo.

---

## 1) Location & Scope

- Use the existing structure: `ShamDevs.EFatoraJo.Sdk/docs/` for SDK docs, and `EFatoraJoConsoleApp/README.md` (or `EFatoraJoConsoleApp/docs/` if needed) for the CLI. Update root `README.md`/`REFERENCE.md` when required instead of adding new root docs.
- Do not create new markdown files in the repo root. If a new guide is needed, place it under the appropriate `docs/` folder with a kebab-case name.
- Update indexes/readmes (e.g., SDK docs index) when adding/removing docs.

## 2) Content Guidelines

- Encoding: UTF-8, no emoji or mojibake. Keep it concise; move deep detail to an “advanced”/appendix file if needed and keep the entry doc short.
- Keep versions/badges accurate (NuGet and `.csproj`). Fix broken links immediately.
- Examples must be synthetic/anonymized (no real credentials, QR, or client data).
- Call out whether content is for the SDK or the CLI, and note platform/requirements where relevant (netstandard2.0+, net8.0).

## 3) File Naming & Structure

- New filenames use kebab-case (`batch-processing-guide.md`). Put examples under `docs/examples/` if applicable, plans under `docs/plans/`, and keep existing READMEs/indices current.
- Do not add docs inside code folders (Enums/Models/Services). Keep docs in their designated doc paths or existing READMEs.

## 4) Updates & Cross-Links

- Update the relevant index/readme when adding/removing a doc (`ShamDevs.EFatoraJo.Sdk/docs/README.md`, etc.).
- Fix links and version numbers alongside the change. Keep relative links.
- If you change data shapes or behaviors, update the examples/JSON in the related docs.

## 5) Security & Hygiene

- No secrets or QR/base64 in docs. Use synthetic data and relative paths.
- Keep docs aligned with actual behavior; test or validate snippets when possible.
