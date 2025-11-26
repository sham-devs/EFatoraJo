# CLI/UX Rules - EFatoraJoConsoleApp

Guidelines for the console app (net8.0) outputs, options, and interactive behavior.

---

## 1) Output & Messaging

- Preserve both output modes: concise text and JSON with `success` plus `result` or `errorType/message/errors`. Do not break the JSON shape.
- Keep messages short and actionable. Avoid ASCII alignment tricks; ensure it reads well on Windows/Linux/macOS.
- Never print credentials or payloads (client/secret, QR/base64, invoice JSON). Use status and IDs only.
- Show where artifacts were written only when the user explicitly requested a file write (e.g., redirected output).

## 2) Options & Flags

- Core options: `--invoice-file` or `--return-file` (mutually exclusive), `--client-id`, `--secret-key`, output mode (text/JSON), plus `--sample`/`--help`/`--version`. No blocking prompts.
- Enforce mutual exclusivity of invoice vs return; require both credentials when submitting.
- Validate file paths exist/readable before processing; fail fast with clear errors.
- Keep exit codes stable (0 success, 1-6 documented errors, 99 unexpected). Any new behavior must respect this map.

## 3) Interactivity

- Interactive mode is optional; clearly state whether it will submit to the live endpoint. Offer a confirmation before sending.
- Provide small, copyable samples via `--sample` with synthetic data only.

## 4) Safety & Paths

- Do not store credentials or QR/output files automatically. Writing to disk must be explicit and user-driven.
- Use `Path.Combine` and cross-platform-safe paths; never assume a specific working directory.
- Avoid creating side files unless required; keep outputs in user-specified or default locations only.

## 5) Testing

- Test with `dotnet test` using faked stdin/stdout/stderr. Assert messages, JSON shape, and exit codes without real network calls.
- Mock HTTP and file I/O; no external submissions in tests.
