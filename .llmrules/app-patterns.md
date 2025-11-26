# Application Patterns - EFatoraJo (.NET)

Patterns for keeping the SDK (netstandard2.0+) and console app (net8.0) consistent, safe, and testable.

---

## 1) Solution Layout

- Projects: `ShamDevs.EFatoraJo.Sdk` (library), `EFatoraJoConsoleApp` (CLI), `ShamDevs.EFatoraJo.Tests` (tests). Avoid scattering files elsewhere.
- Prefer async/await for public workflows. Keep existing `EFatoraJoSdk` entry points stable; add overloads cautiously.
- Serialization: use `System.Text.Json` with current options (snake_case, case-insensitive). UBL is XML via existing helpers; do not introduce temp files for payloads.

## 2) SDK Patterns

- Validate first: run `InvoiceValidator` before UBL generation or submission.
- UBL & encoding: reuse `InvoiceGeneratorService`, `ReturnInvoiceGeneratorService`, and `InvoiceHelper` to produce UBL and Base64; no disk intermediates.
- HTTP: use `HttpClient` with sensible timeouts; send `Client-Id`/`Secret-Key` headers; never log credentials or Base64 payloads. Consider injectable `HttpClient`/handler for testing to avoid per-call instantiation.
- Exceptions: keep the exception taxonomy (`InvoiceValidationException`, `EInvoiceApiException`, etc.) so callers/CLI can handle errors predictably.

## 3) CLI Patterns

- Inputs: JSON files via `--invoice-file` or `--return-file` with `--client-id`/`--secret-key`. Do not allow invoice+return in one call.
- Outputs: maintain text and JSON shapes (success/errorType/exit codes). Keep exit codes stable.
- Interactivity: optional; clearly indicate if a live submission will occur. Provide small synthetic samples.

## 4) Configuration & Secrets

- No hardcoded secrets or endpoints. Use User Secrets for development and environment variables for production. Centralize config keys instead of scattering literals.
- Paths: avoid absolute paths; use `Path.Combine`/`Environment` helpers; respect the working directory or explicit user paths.

## 5) Logging & Errors

- Log actionable errors without sensitive payloads or Base64. Keep CLI messages concise; let exceptions carry technical detail for logs.
- If adding diagnostics, make them opt-in and scrub secrets.

## 6) Code Style

- C# conventions: PascalCase for public types/properties, camelCase for locals/fields, meaningful constants/readonly fields where needed.
- Organize usings (system/third-party/local); avoid in-method usings unless required.
- Use `decimal` for money; avoid `double` for financial calculations.

## 7) Files & Outputs

- Do not touch `bin/obj` or generated assets. Docs live under `ShamDevs.EFatoraJo.Sdk/docs` or existing READMEs. Samples/scripts belong in clear folders (`docs/examples/`, `Output/` if provided) and should stay synthetic and minimal.
