# Security Rules - EFatoraJo

Protect invoice data, credentials, and payloads.

---

## 1) Credentials & Secrets

- Never store `Client-Id` or `Secret-Key` in code or repo. Use User Secrets for development and environment variables for production.
- Do not log credentials or reprint them in CLI output. If a secret leaks, remove the artifact and note the need to rotate keys.

## 2) Sensitive Data

- Treat invoice JSON, Base64 UBL, and QR as sensitive. Do not log or document them.
- Use synthetic data in samples and tests. No real customer names, IDs, or QR payloads.
- Avoid writing temporary payload files; if unavoidable, use temp locations and clean up.

## 3) Network & Config

- HTTPS only; do not disable certificate validation. Set reasonable timeouts.
- Do not hardcode cookies or environment-specific headers. Make endpoints configurable if multiple environments are needed.

## 4) Files & Paths

- Avoid absolute paths; write only to intended locations (working directory or explicit user paths). Respect cross-platform path handling.
- Keep secret/config files out of the repo (user secrets, .env). Ensure `.gitignore` covers them.

## 5) Logging & Output

- Keep logs/output free of Base64 and secrets. Provide concise, actionable errors; keep technical details in exceptions/logs, not user output.
- Make any added diagnostics opt-in and scrub sensitive fields.

## 6) Dependencies

- Prefer well-maintained packages. Avoid adding risky dependencies. Watch for CVEs when updating.
