# Integration Rules - JoFatora

Guidelines for SDK/CLI integration with the JoFatora endpoints (invoice and return submission).

---

## 1) Scope

- Supported paths: submit invoice (`SendFatoraAsync`) and submit return (`SendReturnFatoraAsync`).
- Integration logic lives in the library; the CLI passes data/keys and reports results without adding business rules.

## 2) HTTP & Behavior

- Use HTTPS only with `Client-Id` and `Secret-Key` headers. Do not hardcode cookies or environment-specific headers/endpoints.
- Prefer injectable `HttpClient`/handler for testing; set sensible timeouts; avoid recreating clients per call when possible.
- Send UBL 2.1 as Base64 inside JSON `{ "invoice": "<base64>" }`. Do not log Base64.
- On HTTP failure, throw `EInvoiceApiException` with status code and body, omitting secrets and payloads.

## 3) Error Handling

- Preserve the exception mapping: Validation -> `InvoiceValidationException`; UBL -> `UblGenerationException`; Serialization -> `EInvoiceSerializationException`; HTTP -> `EInvoiceApiException`; other -> `EInvoiceException`.
- Do not convert exceptions to strings only; keep technical context in exceptions, and surface concise messages to CLI/consumers.

## 4) Security & Privacy

- Never log credentials, Base64, or raw invoice content. Use IDs/statuses only.
- Avoid writing temporary payload files; process in memory.
- Keep endpoints configurable (staging/prod) when added; always enforce HTTPS validation.

## 5) Forward Compatibility

- Keep response shapes backward compatible; add fields as optional without breaking consumers.
- Centralize new headers/settings in a helper/builder; ensure testability when adding behavior.

## 6) Testing

- No live HTTP in tests. Use a fake `HttpMessageHandler` to control responses.
- Cover: success path, HTTP failures (status/body), UBL/serialization errors, and validation errors.
