Changelog
=========

1.0.0 - 2025-08-11
------------------

This is the initial public release of the JoFatora .NET Client SDK.

Added
-----

- **Full UBL 2.1 Compliance**: The SDK can generate standards-compliant invoices and credit notes.
- **Async/Await Support**: All core methods are asynchronous to ensure high performance and non-blocking operations.
- **Multiple Invoice Types**: Support for `Standard`, `Income`, and `Special Sales` invoices.
- **Validation**: Comprehensive pre-submission validation to catch common errors before API calls.
- **Error Handling**: Structured exception handling with specific exceptions for validation, API, and serialization issues.
- **Response Handling**: The SDK provides rich response objects that make it easy to check for submission success, errors, and warnings.
- **Models**: Strongly-typed C# models for all JoFatora entities, including `Invoice`, `Supplier`, `Customer`, and `InvoiceDetail`.
- **Test Data Generation**: Built-in utilities for generating valid test invoices, useful for development and integration testing.
- **Multi-Currency Support**: Handles JOD, USD, and EUR with proper decimal formatting.
