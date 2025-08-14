Changelog
=========

All notable changes to the EFatoraJo SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

[1.0.1] - 2025-08-13
--------------------

Security
--------

- **CRITICAL**: Updated System.Text.Json from 8.0.4 to 9.0.8 to address high severity vulnerability ([CVE-2024-43485](https://github.com/advisories/GHSA-8g4q-xg66-9fp4))
- Updated System.Text.Encodings.Web to 9.0.8 for latest security patches
- Updated Microsoft.Bcl.AsyncInterfaces to 9.0.8 to resolve dependency conflicts

Changes
-------

- Upgraded primary JSON processing library to System.Text.Json 9.0.8 for better performance and security
- Maintained Newtonsoft.Json 13.0.3 for backward compatibility with existing model attributes
- Updated all dependencies to stable versions (removed preview packages)
- Improved package dependency management to eliminate version conflicts

New Features
------------

- Enhanced JSON deserialization with support for both System.Text.Json and Newtonsoft.Json
- Added JsonStringEnumConverter support for better enum handling
- Introduced Source Link support for improved debugging experience
- Added comprehensive code analyzers and quality tools
- Enabled package validation for API compatibility assurance

Improvements
------------

- Better error handling for JSON deserialization scenarios
- Enhanced build configuration with separate Debug/Release optimizations
- Streamlined dependencies by removing unused packages
- Added proper symbol generation for debugging support

Removals
--------

- Eliminated legacy Source Code Control (SCC) properties
- Removed unnecessary system packages (Microsoft.CSharp, System.Data.DataSetExtensions)
- Cleaned up duplicate and redundant dependencies

Technical Details
-----------------

- Maintained .NET Standard 2.0 compatibility
- All changes are backward compatible with existing implementations
- No breaking changes to public APIs
- Enhanced performance with optimized JSON processing

[1.0.0] - 2025-08-11
--------------------

This is the initial public release of the JoFatora .NET Client SDK.

Initial Features
----------------

- **Full UBL 2.1 Compliance**: The SDK can generate standards-compliant invoices and credit notes
- **Async/Await Support**: All core methods are asynchronous to ensure high performance and non-blocking operations
- **Multiple Invoice Types**: Support for `Standard`, `Income`, and `Special Sales` invoices
- **Validation**: Comprehensive pre-submission validation to catch common errors before API calls
- **Error Handling**: Structured exception handling with specific exceptions for validation, API, and serialization issues
- **Response Handling**: The SDK provides rich response objects that make it easy to check for submission success, errors, and warnings
- **Models**: Strongly-typed C# models for all JoFatora entities, including `Invoice`, `Supplier`, `Customer`, and `InvoiceDetail`
- **Test Data Generation**: Built-in utilities for generating valid test invoices, useful for development and integration testing
- **Multi-Currency Support**: Handles JOD, USD, and EUR with proper decimal formatting
