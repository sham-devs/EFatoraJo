# Testing Rules - EFatoraJo

What to run and cover before declaring work complete.

---

## 1) What to Run

- `dotnet test EFatoraJo.sln` or target the impacted projects (`ShamDevs.EFatoraJo.Tests`). Use the appropriate configuration (Debug/Release) for the task.
- `dotnet build` to ensure compilation; `dotnet format` on touched projects if formatting is needed.

## 2) Execution Rules

- No external/network calls in tests. Use `HttpMessageHandler` fakes/mocks for JoFatora responses.
- Keep filesystem writes in temp locations; avoid writing to repo output folders during tests.
- Make outputs deterministic: avoid time/order dependence or uncontrolled randomness.

## 3) Coverage Areas

- Validation: pass/fail for invoices and returns, including negative values for returns.
- UBL/serialization: generate valid XML/Base64; handle serialization failures.
- Integration: success path, HTTP failures (status/body), JSON/serialization errors in `EInvoiceResponse`.
- CLI: if changed, test exit codes, text/JSON output shapes, and error messages without real network calls.

## 4) Fixtures & Data

- Use synthetic builders/fixtures from existing test folders (`ShamDevs.EFatoraJo.Tests/Builders`). No real data.
- Keep test artifacts in temp dirs; do not write to `Output/` or other shipped folders.

## 5) Completion Criteria

- State what you ran and what was skipped (with reasons). Do not mark done until required checks pass or are explicitly deferred.
