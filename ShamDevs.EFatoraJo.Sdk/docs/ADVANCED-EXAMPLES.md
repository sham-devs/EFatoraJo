# EFatoraJo Advanced Usage Examples

## Overview

This guide provides advanced usage examples for EFatoraJo SDK and Console Application, including complex scenarios, integration patterns, and best practices for production environments.

---

## Complex Invoice Scenarios

### 1. Multi-Line Invoice with Discounts

```csharp
using ShamDevs.EFatoraJo;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Enums;

// Create supplier
var supplier = new Supplier(
    taxVATNumber: "123456789",
    incomeSourceSequence: "62010",
    registeredSupplierName: "Advanced Trading Co.");

// Create customer
var customer = new Customer("Premium Customer Ltd.")
{
    IdentificationNumber = "987654321",
    IdentificationType = IdentificationType.TN,
    City = CityCode.Amman,
    PostalCode = "11118",
    PhoneNumber = "+962791234567"
};

// Create multiple invoice lines with discounts
var invoiceDetails = new List<InvoiceDetail>
{
    new InvoiceDetail("LINE-1", TaxCategoryCode.S, "Premium Laptop")
    {
        Quantity = 2,
        UnitPriceBeforeTax = 1500.000m,
        DiscountAmount = 150.000m, // 10% discount
        TotalBeforeTax = 2850.000m, // (1500 * 2) - 150
        TaxAmount = 456.000m, // 16% VAT on 2850
        TotalIncludingTax = 3306.000m,
        TaxRate = 0.16m
    },
    new InvoiceDetail("LINE-2", TaxCategoryCode.S8, "Software License")
    {
        Quantity = 5,
        UnitPriceBeforeTax = 200.000m,
        DiscountAmount = 20.000m, // 10% discount
        TotalBeforeTax = 980.000m, // (200 * 5) - 20
        TaxAmount = 78.400m, // 8% VAT on 980
        TotalIncludingTax = 1058.400m,
        TaxRate = 0.08m
    },
    new InvoiceDetail("LINE-3", TaxCategoryCode.Z, "Training Services")
    {
        Quantity = 10,
        UnitPriceBeforeTax = 100.000m,
        TotalBeforeTax = 1000.000m,
        TaxAmount = 0.000m, // Zero-rated
        TotalIncludingTax = 1000.000m,
        TaxRate = 0.00m
    }
};

// Calculate totals
var totals = new InvoiceTotals
{
    TotalBeforeDiscount = 5000.000m, // 3000 + 1000 + 1000
    TotalDiscountAmount = 170.000m, // 150 + 20
    TotalVATAmount = 534.400m, // 456 + 78.4 + 0
    TotalInvoiceAmount = 5364.400m, // 4830 + 534.4
    FinalPayableAmount = 5364.400m
};

// Create invoice
var invoice = new Invoice(
    invoiceNumber: "ADV-2024-001",
    uniqueSerialNumber: Guid.NewGuid().ToString(),
    invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
    paymentType: InvoicePaymentTypeCode.LocalGeneralSalesCredit,
    supplier: supplier,
    customer: customer,
    invoiceTotals: totals,
    invoiceDetails: invoiceDetails,
    type: InvoiceType.GeneralSales)
{
    InvoiceNote = "Multi-line invoice with volume discounts",
    Currency = CurrencyCode.JOD
};

// Submit invoice
try
{
    var response = await EFatoraJoSdk.SendFatoraAsync(invoice, clientId, secretKey);
    Console.WriteLine($"Invoice submitted: {response.InvoiceNumber}");
}
catch (InvoiceValidationException ex)
{
    Console.WriteLine($"Validation failed: {string.Join(", ", ex.ValidationErrors)}");
}
```

### 2. Special Sales Invoice with Mixed Tax Categories

```csharp
// Create special sales invoice with different tax categories
var specialInvoiceDetails = new List<InvoiceDetail>
{
    new InvoiceDetail("LINE-1", TaxCategoryCode.S, "Luxury Vehicle")
    {
        Quantity = 1,
        UnitPriceBeforeTax = 50000.000m,
        TotalBeforeTax = 50000.000m,
        TaxAmount = 8000.000m, // 16% VAT
        SpecialTaxAmount = 2500.000m, // 5% special tax
        TotalIncludingTax = 60500.000m,
        TaxRate = 0.16m
    },
    new InvoiceDetail("LINE-2", TaxCategoryCode.S5, "Tobacco Products")
    {
        Quantity = 100,
        UnitPriceBeforeTax = 50.000m,
        TotalBeforeTax = 5000.000m,
        TaxAmount = 250.000m, // 5% VAT
        SpecialTaxAmount = 500.000m, // 10% special tax
        TotalIncludingTax = 5750.000m,
        TaxRate = 0.05m
    },
    new InvoiceDetail("LINE-3", TaxCategoryCode.S8, "Telecommunications")
    {
        Quantity = 12,
        UnitPriceBeforeTax = 100.000m,
        TotalBeforeTax = 1200.000m,
        TaxAmount = 96.000m, // 8% VAT
        SpecialTaxAmount = 120.000m, // 10% special tax
        TotalIncludingTax = 1416.000m,
        TaxRate = 0.08m
    }
};

var specialTotals = new InvoiceTotals
{
    TotalBeforeDiscount = 56200.000m,
    TotalDiscountAmount = 0.000m,
    TotalVATAmount = 8346.000m, // 8000 + 250 + 96
    TotalSpecialTaxAmount = 3120.000m, // 2500 + 500 + 120
    TotalInvoiceAmount = 67666.000m,
    FinalPayableAmount = 67666.000m
};

var specialInvoice = new Invoice(
    invoiceNumber: "SPC-2024-001",
    uniqueSerialNumber: Guid.NewGuid().ToString(),
    invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
    paymentType: InvoicePaymentTypeCode.LocalSpecialSalesCash,
    supplier: supplier,
    customer: customer,
    invoiceTotals: specialTotals,
    invoiceDetails: specialInvoiceDetails,
    type: InvoiceType.SpecialSales)
{
    InvoiceNote = "Special sales with mixed tax categories",
    Currency = CurrencyCode.JOD
};
```

### 3. Multi-Currency Invoice

```csharp
// USD Invoice
var usdInvoice = new Invoice(
    invoiceNumber: "USD-2024-001",
    uniqueSerialNumber: Guid.NewGuid().ToString(),
    invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
    paymentType: InvoicePaymentTypeCode.LocalGeneralSalesCash,
    supplier: supplier,
    customer: customer,
    invoiceTotals: new InvoiceTotals
    {
        TotalBeforeDiscount = 1000.000m,
        TotalDiscountAmount = 0.000m,
        TotalVATAmount = 160.000m,
        TotalInvoiceAmount = 1160.000m,
        FinalPayableAmount = 1160.000m
    },
    invoiceDetails: new List<InvoiceDetail>
    {
        new InvoiceDetail("LINE-1", TaxCategoryCode.S, "Software Service")
        {
            Quantity = 1,
            UnitPriceBeforeTax = 1000.000m,
            TotalBeforeTax = 1000.000m,
            TaxAmount = 160.000m,
            TotalIncludingTax = 1160.000m,
            TaxRate = 0.16m
        }
    },
    type: InvoiceType.GeneralSales)
{
    Currency = CurrencyCode.USD,
    InvoiceNote = "USD invoice for international client"
};

// EUR Invoice
var eurInvoice = new Invoice(
    invoiceNumber: "EUR-2024-001",
    uniqueSerialNumber: Guid.NewGuid().ToString(),
    invoiceDate: DateTime.Now.ToString("yyyy-MM-dd"),
    paymentType: InvoicePaymentTypeCode.LocalGeneralSalesCash,
    supplier: supplier,
    customer: customer,
    invoiceTotals: new InvoiceTotals
    {
        TotalBeforeDiscount = 1000.000m,
        TotalDiscountAmount = 0.000m,
        TotalVATAmount = 160.000m,
        TotalInvoiceAmount = 1160.000m,
        FinalPayableAmount = 1160.000m
    },
    invoiceDetails: new List<InvoiceDetail>
    {
        new InvoiceDetail("LINE-1", TaxCategoryCode.S, "Consulting Service")
        {
            Quantity = 1,
            UnitPriceBeforeTax = 1000.000m,
            TotalBeforeTax = 1000.000m,
            TaxAmount = 160.000m,
            TotalIncludingTax = 1160.000m,
            TaxRate = 0.16m
        }
    },
    type: InvoiceType.GeneralSales)
{
    Currency = CurrencyCode.EUR,
    InvoiceNote = "EUR invoice for European client"
};
```

---

## Batch Processing Implementation

### 1. High-Volume Batch Processor

```csharp
using System.Collections.Concurrent;
using System.Threading.Channels;

public class BatchInvoiceProcessor
{
    private readonly string _clientId;
    private readonly string _secretKey;
    private readonly ILogger<BatchInvoiceProcessor> _logger;
    private readonly SemaphoreSlim _semaphore;
    private readonly Channel<Invoice> _invoiceChannel;

    public BatchInvoiceProcessor(
        string clientId, 
        string secretKey, 
        ILogger<BatchInvoiceProcessor> logger,
        int maxConcurrency = 10)
    {
        _clientId = clientId;
        _secretKey = secretKey;
        _logger = logger;
        _semaphore = new SemaphoreSlim(maxConcurrency);
        
        // Create channel for invoice processing
        var options = new BoundedChannelOptions(1000); // Max 1000 pending invoices
        _invoiceChannel = Channel.CreateBounded<Invoice>(options);
    }

    public async Task ProcessBatchAsync(IEnumerable<Invoice> invoices)
    {
        var results = new ConcurrentBag<ProcessingResult>();
        var tasks = new List<Task>();

        foreach (var invoice in invoices)
        {
            await _invoiceChannel.Writer.WriteAsync(invoice);
        }

        // Start consumer tasks
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            tasks.Add(Task.Run(() => ProcessInvoicesConsumer(results)));
        }

        // Signal completion and wait for all tasks
        _invoiceChannel.Writer.Complete();
        await Task.WhenAll(tasks);

        return results.ToList();
    }

    private async Task ProcessInvoicesConsumer(ConcurrentBag<ProcessingResult> results)
    {
        await foreach (var invoice in _invoiceChannel.Reader.ReadAllAsync())
        {
            await _semaphore.WaitAsync();
            try
            {
                var result = await ProcessSingleInvoiceAsync(invoice);
                results.Add(result);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    private async Task<ProcessingResult> ProcessSingleInvoiceAsync(Invoice invoice)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await EFatoraJoSdk.SendFatoraAsync(invoice, _clientId, _secretKey);
            
            stopwatch.Stop();
            
            if (response.IsSuccessfullySubmitted())
            {
                _logger.LogInformation(
                    "Invoice {InvoiceNumber} processed successfully in {ElapsedMs}ms",
                    invoice.InvoiceNumber, stopwatch.ElapsedMilliseconds);
                
                return new ProcessingResult
                {
                    InvoiceNumber = invoice.InvoiceNumber,
                    Success = true,
                    Response = response,
                    ProcessingTime = stopwatch.Elapsed
                };
            }
            else
            {
                var errors = response.GetFormattedErrors();
                _logger.LogWarning(
                    "Invoice {InvoiceNumber} failed: {Errors}",
                    invoice.InvoiceNumber, errors);
                
                return new ProcessingResult
                {
                    InvoiceNumber = invoice.InvoiceNumber,
                    Success = false,
                    Errors = errors,
                    ProcessingTime = stopwatch.Elapsed
                };
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, 
                "Invoice {InvoiceNumber} failed with exception after {ElapsedMs}ms",
                invoice.InvoiceNumber, stopwatch.ElapsedMilliseconds);
            
            return new ProcessingResult
            {
                InvoiceNumber = invoice.InvoiceNumber,
                Success = false,
                Error = ex.Message,
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }
}

public class ProcessingResult
{
    public string InvoiceNumber { get; set; }
    public bool Success { get; set; }
    public EInvoiceResponse? Response { get; set; }
    public string? Errors { get; set; }
    public string? Error { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}
```

### 2. Retry Logic with Exponential Backoff

```csharp
public class ResilientInvoiceProcessor
{
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _baseDelay = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _maxDelay = TimeSpan.FromMinutes(1);

    public async Task<SubmissionResult> SubmitWithRetryAsync(Invoice invoice, string clientId, string secretKey)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                var response = await EFatoraJoSdk.SendFatoraAsync(invoice, clientId, secretKey);
                return new SubmissionResult { Success = true, Response = response };
            }
            catch (EInvoiceApiException ex) when (IsRetryableError(ex) && attempt < _maxRetries)
            {
                var delay = CalculateBackoffDelay(attempt);
                
                _logger.LogWarning(
                    "Attempt {Attempt}/{MaxRetries} failed for invoice {InvoiceNumber}. " +
                    "Retrying in {DelaySeconds}s. Error: {Error}",
                    attempt, _maxRetries, invoice.InvoiceNumber, 
                    delay.TotalSeconds, ex.Message);
                
                await Task.Delay(delay);
            }
            catch (Exception ex) when (attempt == _maxRetries)
            {
                _logger.LogError(ex, 
                    "All {MaxRetries} attempts failed for invoice {InvoiceNumber}",
                    _maxRetries, invoice.InvoiceNumber);
                
                return new SubmissionResult 
                { 
                    Success = false, 
                    Error = $"Failed after {_maxRetries} attempts: {ex.Message}" 
                };
            }
        }

        return new SubmissionResult { Success = false, Error = "Unexpected error" };
    }

    private bool IsRetryableError(EInvoiceApiException ex)
    {
        // Retry on network errors, timeouts, server errors
        return ex.StatusCode switch
        {
            >= 500 => true, // Server errors
            408 => true, // Request timeout
            429 => true, // Rate limited
            _ => false
        };
    }

    private TimeSpan CalculateBackoffDelay(int attempt)
    {
        var delay = TimeSpan.FromTicks(_baseDelay.Ticks * Math.Pow(2, attempt - 1));
        return delay > _maxDelay ? _maxDelay : delay;
    }
}
```

---

## Web Application Integration

### 1. ASP.NET Core Service with Dependency Injection

```csharp
// Configure EFatoraJo options
public class EFatoraOptions
{
    public string ClientId { get; set; }
    public string SecretKey { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

// Service interface
public interface IInvoiceSubmissionService
{
    Task<SubmissionResult> SubmitInvoiceAsync(InvoiceDto invoiceDto);
    Task<SubmissionResult> SubmitReturnInvoiceAsync(ReturnInvoiceDto returnDto);
    Task<InvoiceStatusResult> GetInvoiceStatusAsync(string invoiceNumber);
}

// Service implementation
public class InvoiceSubmissionService : IInvoiceSubmissionService
{
    private readonly EFatoraOptions _options;
    private readonly ILogger<InvoiceSubmissionService> _logger;
    private readonly ICacheService _cache;
    private readonly ITelemetryService _telemetry;

    public InvoiceSubmissionService(
        IOptions<EFatoraOptions> options,
        ILogger<InvoiceSubmissionService> logger,
        ICacheService cache,
        ITelemetryService telemetry)
    {
        _options = options.Value;
        _logger = logger;
        _cache = cache;
        _telemetry = telemetry;
    }

    public async Task<SubmissionResult> SubmitInvoiceAsync(InvoiceDto invoiceDto)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString();
        
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["InvoiceNumber"] = invoiceDto.InvoiceNumber
        });

        try
        {
            // Check cache for duplicate submission
            var cacheKey = $"invoice_{invoiceDto.InvoiceNumber}";
            if (await _cache.GetAsync(cacheKey) != null)
            {
                _logger.LogWarning("Duplicate invoice submission attempt: {InvoiceNumber}", 
                    invoiceDto.InvoiceNumber);
                return new SubmissionResult 
                { 
                    Success = false, 
                    Error = "Invoice already submitted" 
                };
            }

            var invoice = MapToInvoice(invoiceDto);
            
            // Pre-validate
            InvoiceValidator.ValidateInvoice(invoice);
            
            // Submit with retry logic
            var processor = new ResilientInvoiceProcessor();
            var result = await processor.SubmitWithRetryAsync(
                invoice, _options.ClientId, _options.SecretKey);

            if (result.Success)
            {
                // Cache successful submission
                await _cache.SetAsync(cacheKey, result.Response, TimeSpan.FromHours(24));
                
                // Track metrics
                _telemetry.TrackMetric("InvoiceSubmissionSuccess", 1, new Dictionary<string, string>
                {
                    ["InvoiceType"] = invoice.Type.ToString(),
                    ["Currency"] = invoice.Currency.ToString()
                });
            }
            else
            {
                _telemetry.TrackMetric("InvoiceSubmissionFailure", 1, new Dictionary<string, string>
                {
                    ["ErrorType"] = "SubmissionFailed"
                });
            }

            stopwatch.Stop();
            _telemetry.TrackMetric("InvoiceSubmissionDuration", stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
            {
                ["Success"] = result.Success.ToString()
            });

            return result;
        }
        catch (InvoiceValidationException ex)
        {
            _telemetry.TrackMetric("InvoiceSubmissionFailure", 1, new Dictionary<string, string>
            {
                ["ErrorType"] = "ValidationError"
            });
            
            return new SubmissionResult 
            { 
                Success = false, 
                Errors = ex.ValidationErrors 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error submitting invoice {InvoiceNumber}", 
                invoiceDto.InvoiceNumber);
            
            _telemetry.TrackMetric("InvoiceSubmissionFailure", 1, new Dictionary<string, string>
            {
                ["ErrorType"] = "UnexpectedError"
            });
            
            return new SubmissionResult 
            { 
                Success = false, 
                Error = "Internal error occurred" 
            };
        }
    }

    private Invoice MapToInvoice(InvoiceDto dto)
    {
        // Implementation maps DTO to domain model
        return new Invoice(
            invoiceNumber: dto.InvoiceNumber,
            uniqueSerialNumber: dto.UniqueSerialNumber,
            invoiceDate: dto.InvoiceDate,
            paymentType: Enum.Parse<InvoicePaymentTypeCode>(dto.PaymentType),
            supplier: new Supplier(dto.Supplier.TaxVATNumber, dto.Supplier.IncomeSourceSequence, dto.Supplier.RegisteredSupplierName),
            customer: new Customer(dto.Customer.Name)
            {
                IdentificationNumber = dto.Customer.IdentificationNumber,
                IdentificationType = Enum.Parse<IdentificationType>(dto.Customer.IdentificationType),
                PhoneNumber = dto.Customer.PhoneNumber,
                City = Enum.Parse<CityCode>(dto.Customer.City),
                PostalCode = dto.Customer.PostalCode
            },
            invoiceTotals: new InvoiceTotals
            {
                TotalBeforeDiscount = dto.Totals.TotalBeforeDiscount,
                TotalDiscountAmount = dto.Totals.TotalDiscountAmount,
                TotalVATAmount = dto.Totals.TotalVATAmount,
                TotalSpecialTaxAmount = dto.Totals.TotalSpecialTaxAmount,
                TotalInvoiceAmount = dto.Totals.TotalInvoiceAmount,
                FinalPayableAmount = dto.Totals.FinalPayableAmount
            },
            invoiceDetails: dto.InvoiceLines.Select(line => new InvoiceDetail(line.Id, Enum.Parse<TaxCategoryCode>(line.TaxCategory), line.Description)
            {
                Quantity = line.Quantity,
                UnitPriceBeforeTax = line.UnitPriceBeforeTax,
                TotalBeforeTax = line.TotalBeforeTax,
                DiscountAmount = line.DiscountAmount,
                TaxAmount = line.TaxAmount,
                SpecialTaxAmount = line.SpecialTaxAmount,
                TotalIncludingTax = line.TotalIncludingTax,
                TaxRate = line.TaxRate
            }).ToList(),
            type: Enum.Parse<InvoiceType>(dto.InvoiceType))
        {
            Currency = Enum.Parse<CurrencyCode>(dto.Currency),
            InvoiceNote = dto.InvoiceNote
        };
    }
}

// DTOs for API
public class InvoiceDto
{
    public string InvoiceNumber { get; set; }
    public string UniqueSerialNumber { get; set; }
    public string InvoiceDate { get; set; }
    public string InvoiceType { get; set; }
    public string PaymentType { get; set; }
    public string Currency { get; set; }
    public string InvoiceNote { get; set; }
    public SupplierDto Supplier { get; set; }
    public CustomerDto Customer { get; set; }
    public List<InvoiceLineDto> InvoiceLines { get; set; }
    public InvoiceTotalsDto Totals { get; set; }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceSubmissionService _submissionService;

    public InvoicesController(IInvoiceSubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    [HttpPost("submit")]
    public async Task<ActionResult<SubmissionResult>> SubmitInvoice([FromBody] InvoiceDto invoiceDto)
    {
        try
        {
            var result = await _submissionService.SubmitInvoiceAsync(invoiceDto);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SubmissionResult 
            { 
                Success = false, 
                Error = "Internal server error" 
            });
        }
    }

    [HttpPost("submit-return")]
    public async Task<ActionResult<SubmissionResult>> SubmitReturnInvoice([FromBody] ReturnInvoiceDto returnDto)
    {
        var result = await _submissionService.SubmitReturnInvoiceAsync(returnDto);
        
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpGet("status/{invoiceNumber}")]
    public async Task<ActionResult<InvoiceStatusResult>> GetInvoiceStatus(string invoiceNumber)
    {
        var result = await _submissionService.GetInvoiceStatusAsync(invoiceNumber);
        
        if (result != null)
        {
            return Ok(result);
        }
        else
        {
            return NotFound();
        }
    }
}
```

### 2. Background Service for Queue Processing

```csharp
public class InvoiceQueueProcessor : BackgroundService
{
    private readonly IInvoiceQueue _queue;
    private readonly IInvoiceSubmissionService _submissionService;
    private readonly ILogger<InvoiceQueueProcessor> _logger;
    private readonly IOptions<QueueProcessorOptions> _options;

    public InvoiceQueueProcessor(
        IInvoiceQueue queue,
        IInvoiceSubmissionService submissionService,
        ILogger<InvoiceQueueProcessor> logger,
        IOptions<QueueProcessorOptions> options)
    {
        _queue = queue;
        _submissionService = submissionService;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Invoice queue processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var invoice = await _queue.DequeueAsync(stoppingToken);
                if (invoice != null)
                {
                    await ProcessInvoiceAsync(invoice, stoppingToken);
                }
                else
                {
                    // No invoice in queue, wait before next check
                    await Task.Delay(_options.PollInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing invoice from queue");
                await Task.Delay(_options.ErrorDelay, stoppingToken);
            }
        }

        _logger.LogInformation("Invoice queue processor stopped");
    }

    private async Task ProcessInvoiceAsync(QueuedInvoice queuedInvoice, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["QueueId"] = queuedInvoice.Id.ToString(),
            ["InvoiceNumber"] = queuedInvoice.InvoiceNumber
        });

        try
        {
            _logger.LogInformation("Processing queued invoice {InvoiceNumber}", queuedInvoice.InvoiceNumber);
            
            var result = await _submissionService.SubmitInvoiceAsync(queuedInvoice.InvoiceDto);
            
            if (result.Success)
            {
                await _queue.MarkAsProcessedAsync(queuedInvoice.Id);
                _logger.LogInformation("Successfully processed invoice {InvoiceNumber}", queuedInvoice.InvoiceNumber);
            }
            else
            {
                await _queue.MarkAsFailedAsync(queuedInvoice.Id, result.Error);
                _logger.LogWarning("Failed to process invoice {InvoiceNumber}: {Error}", 
                    queuedInvoice.InvoiceNumber, result.Error);
            }
        }
        catch (Exception ex)
        {
            await _queue.MarkAsFailedAsync(queuedInvoice.Id, ex.Message);
            _logger.LogError(ex, "Exception processing invoice {InvoiceNumber}", queuedInvoice.InvoiceNumber);
        }
    }
}

public class QueueProcessorOptions
{
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan ErrorDelay { get; set; } = TimeSpan.FromMinutes(1);
}
```

---

## Testing Strategies

### 1. Comprehensive Test Suite

```csharp
using Xunit;
using ShamDevs.EFatoraJo.Test.Builders;

public class InvoiceSubmissionTests
{
    private readonly Mock<ILogger<InvoiceSubmissionService>> _mockLogger;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<ITelemetryService> _mockTelemetry;
    private readonly IOptions<EFatoraOptions> _options;
    private readonly InvoiceSubmissionService _service;

    public InvoiceSubmissionTests()
    {
        _mockLogger = new Mock<ILogger<InvoiceSubmissionService>>();
        _mockCache = new Mock<ICacheService>();
        _mockTelemetry = new Mock<ITelemetryService>();
        
        _options = Options.Create(new EFatoraOptions
        {
            ClientId = "test-client-id",
            SecretKey = "test-secret-key"
        });

        _service = new InvoiceSubmissionService(
            _options, _mockLogger.Object, _mockCache.Object, _mockTelemetry.Object);
    }

    [Fact]
    public async Task SubmitInvoice_ValidGeneralSales_ReturnsSuccess()
    {
        // Arrange
        var invoiceDto = new InvoiceBuilder()
            .WithGeneralSalesType()
            .WithValidCustomer()
            .WithValidSupplier()
            .WithSingleLineItem()
            .BuildDto();

        _mockCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(null);

        // Act
        var result = await _service.SubmitInvoiceAsync(invoiceDto);

        // Assert
        Assert.True(result.Success);
        _mockCache.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<EInvoiceResponse>(), It.IsAny<TimeSpan>()), Times.Once);
        _mockTelemetry.Verify(x => x.TrackMetric("InvoiceSubmissionSuccess", It.IsAny<int>(), It.IsAny<IDictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task SubmitInvoice_InvalidData_ReturnsValidationError()
    {
        // Arrange
        var invoiceDto = new InvoiceBuilder()
            .WithInvalidDate() // Future date
            .WithValidCustomer()
            .WithValidSupplier()
            .BuildDto();

        // Act
        var result = await _service.SubmitInvoiceAsync(invoiceDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Contains("InvoiceDate", result.Errors);
    }

    [Fact]
    public async Task SubmitInvoice_DuplicateSubmission_ReturnsError()
    {
        // Arrange
        var invoiceDto = new InvoiceBuilder()
            .WithGeneralSalesType()
            .WithValidCustomer()
            .WithValidSupplier()
            .BuildDto();

        var cachedResponse = new EInvoiceResponse { InvoiceNumber = "INV-001" };
        _mockCache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(cachedResponse);

        // Act
        var result = await _service.SubmitInvoiceAsync(invoiceDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invoice already submitted", result.Error);
    }

    [Theory]
    [InlineData(InvoiceType.GeneralSales, 2)]
    [InlineData(InvoiceType.SpecialSales, 3)]
    [InlineData(InvoiceType.Income, 1)]
    public async Task SubmitInvoice_DifferentTypes_CalculatesCorrectTotals(InvoiceType invoiceType, int expectedLineCount)
    {
        // Arrange
        var invoiceDto = new InvoiceBuilder()
            .WithInvoiceType(invoiceType)
            .WithValidCustomer()
            .WithValidSupplier()
            .WithMultipleLineItems(expectedLineCount)
            .BuildDto();

        // Act
        var result = await _service.SubmitInvoiceAsync(invoiceDto);

        // Assert
        Assert.True(result.Success);
        // Additional assertions based on invoice type
        switch (invoiceType)
        {
            case InvoiceType.Income:
                Assert.All(invoiceDto.InvoiceLines, line => line.TaxAmount == 0);
                break;
            case InvoiceType.SpecialSales:
                Assert.All(invoiceDto.InvoiceLines, line => line.SpecialTaxAmount > 0);
                break;
        }
    }
}
```

### 2. Performance Testing

```csharp
using BenchmarkDotNet.Attributes;
using System.Threading.Tasks.Dataflow;

[MemoryDiagnoser]
[SimpleJob]
public class InvoiceSubmissionBenchmark
{
    private readonly EFatoraOptions _options;
    private readonly List<Invoice> _testInvoices;

    public InvoiceSubmissionBenchmark()
    {
        _options = new EFatoraOptions
        {
            ClientId = "benchmark-client",
            SecretKey = "benchmark-secret"
        };

        // Generate test data
        _testInvoices = Enumerable.Range(1, 1000)
            .Select(i => RandomInvoiceGenerator.GenerateRandomInvoice(
                invoiceType: InvoiceType.GeneralSales))
            .ToList();
    }

    [Benchmark]
    public async Task<List<SubmissionResult>> SubmitSequential()
    {
        var results = new List<SubmissionResult>();
        
        foreach (var invoice in _testInvoices)
        {
            var result = await EFatoraJoSdk.SendFatoraAsync(
                invoice, _options.ClientId, _options.SecretKey);
            results.Add(new SubmissionResult { Success = result.IsSuccessfullySubmitted() });
        }
        
        return results;
    }

    [Benchmark]
    public async Task<List<SubmissionResult>> SubmitParallel()
    {
        var results = new ConcurrentBag<SubmissionResult>();
        
        await Parallel.ForEachAsync(_testInvoices, async invoice =>
        {
            var result = await EFatoraJoSdk.SendFatoraAsync(
                invoice, _options.ClientId, _options.SecretKey);
            results.Add(new SubmissionResult { Success = result.IsSuccessfullySubmitted() });
        });
        
        return results.ToList();
    }

    [Benchmark]
    public async Task<List<SubmissionResult>> SubmitWithActionBlock()
    {
        var results = new ConcurrentBag<SubmissionResult>();
        
        var actionBlock = new ActionBlock<Invoice>(async invoice =>
        {
            var result = await EFatoraJoSdk.SendFatoraAsync(
                invoice, _options.ClientId, _options.SecretKey);
            results.Add(new SubmissionResult { Success = result.IsSuccessfullySubmitted() });
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            BoundedCapacity = 100
        });

        foreach (var invoice in _testInvoices)
        {
            await actionBlock.SendAsync(invoice);
        }

        actionBlock.Complete();
        await actionBlock.Completion;
        
        return results.ToList();
    }
}
```

---

## Monitoring and Observability

### 1. Custom Telemetry Integration

```csharp
public class EFatoraJoTelemetry
{
    private readonly ITelemetryClient _telemetryClient;
    private readonly ILogger<EFatoraJoTelemetry> _logger;

    public EFatoraJoTelemetry(ITelemetryClient telemetryClient, ILogger<EFatoraJoTelemetry> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    public void TrackInvoiceSubmission(Invoice invoice, SubmissionResult result, TimeSpan duration)
    {
        var properties = new Dictionary<string, string>
        {
            ["InvoiceType"] = invoice.Type.ToString(),
            ["Currency"] = invoice.Currency.ToString(),
            ["LineItemCount"] = invoice.InvoiceDetails.Count.ToString(),
            ["Success"] = result.Success.ToString(),
            ["HasDiscount"] = (invoice.InvoiceDetails.Any(d => d.DiscountAmount.HasValue)).ToString()
        };

        var metrics = new Dictionary<string, double>
        {
            ["SubmissionDurationMs"] = duration.TotalMilliseconds,
            ["InvoiceTotal"] = (double)invoice.InvoiceTotals.TotalInvoiceAmount,
            ["TaxAmount"] = (double)invoice.InvoiceTotals.TotalVATAmount
        };

        // Track custom event
        _telemetryClient.TrackEvent("InvoiceSubmission", properties, metrics);

        // Log structured data
        if (result.Success)
        {
            _logger.LogInformation(
                "Invoice {InvoiceNumber} submitted successfully in {DurationMs}ms",
                invoice.InvoiceNumber, duration.TotalMilliseconds);
        }
        else
        {
            _logger.LogWarning(
                "Invoice {InvoiceNumber} submission failed: {Error}",
                invoice.InvoiceNumber, result.Error ?? "Unknown error");
        }
    }

    public void TrackValidationErrors(InvoiceValidationException ex)
    {
        var properties = new Dictionary<string, string>
        {
            ["ErrorCount"] = ex.ValidationErrors.Count.ToString()
        };

        foreach (var error in ex.ValidationErrors)
        {
            properties[$"ValidationError_{error.GetHashCode()}"] = error;
        }

        _telemetryClient.TrackEvent("InvoiceValidationFailed", properties);
        _logger.LogWarning("Invoice validation failed with {ErrorCount} errors", ex.ValidationErrors.Count);
    }

    public void TrackApiError(EInvoiceApiException ex, string invoiceNumber)
    {
        var properties = new Dictionary<string, string>
        {
            ["HttpStatusCode"] = ex.StatusCode.ToString(),
            ["InvoiceNumber"] = invoiceNumber,
            ["IsRetryable"] = ex.IsRetryable.ToString()
        };

        _telemetryClient.TrackEvent("ApiError", properties);
        _logger.LogError(ex, "API error for invoice {InvoiceNumber}: {StatusCode}", 
            invoiceNumber, ex.StatusCode);
    }
}
```

### 2. Health Check Implementation

```csharp
public class EFatoraJoHealthCheck : IHealthCheck
{
    private readonly EFatoraOptions _options;
    private readonly ILogger<EFatoraJoHealthCheck> _logger;

    public EFatoraJoHealthCheck(IOptions<EFatoraOptions> options, ILogger<EFatoraJoHealthCheck> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a minimal test invoice
            var testInvoice = RandomInvoiceGenerator.GenerateRandomInvoice(
                invoiceType: InvoiceType.Income); // Use income to avoid tax calculations

            // Set a unique test identifier
            testInvoice.InvoiceNumber = $"HEALTH-CHECK-{DateTime.Now:yyyyMMdd-HHmmss}";

            // Attempt submission
            var response = await EFatoraJoSdk.SendFatoraAsync(
                testInvoice, _options.ClientId, _options.SecretKey);

            if (response.IsSuccessfullySubmitted())
            {
                return HealthCheckResult.Healthy("EFatoraJo API is accessible and responding");
            }
            else
            {
                return HealthCheckResult.Degraded(
                    $"EFatoraJo API responded with errors: {response.GetFormattedErrors()}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy($"EFatoraJo API health check failed: {ex.Message}");
        }
    }
}

// Register health check in Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddCheck<EFatoraJoHealthCheck>("efatorajo");
}
```

---

## Security Best Practices

### 1. Credential Management

```csharp
public class SecureCredentialProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecureCredentialProvider> _logger;

    public SecureCredentialProvider(IConfiguration configuration, ILogger<SecureCredentialProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(string ClientId, string SecretKey)> GetCredentialsAsync()
    {
        try
        {
            // Try environment variables first (production)
            var clientId = Environment.GetEnvironmentVariable("EFATORA_CLIENT_ID");
            var secretKey = Environment.GetEnvironmentVariable("EFATORA_SECRET_KEY");

            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(secretKey))
            {
                return (clientId, secretKey);
            }

            // Fallback to Azure Key Vault (if configured)
            if (bool.Parse(Environment.GetEnvironmentVariable("USE_AZURE_KEYVAULT") ?? "false"))
            {
                return await GetFromAzureKeyVaultAsync();
            }

            // Fallback to user secrets (development only)
            clientId = _configuration["EFatora:ClientId"];
            secretKey = _configuration["EFatora:SecretKey"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("EFatoraJo credentials not configured");
            }

            return (clientId, secretKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EFatoraJo credentials");
            throw;
        }
    }

    private async Task<(string ClientId, string SecretKey)> GetFromAzureKeyVaultAsync()
    {
        // Implementation for Azure Key Vault integration
        var keyVaultUri = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URI");
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");

        // Use Azure SDK to retrieve credentials
        // Implementation omitted for brevity
        throw new NotImplementedException("Azure Key Vault integration not implemented");
    }
}
```

### 2. Input Sanitization

```csharp
public class InvoiceSanitizer
{
    public static InvoiceDto Sanitize(InvoiceDto invoiceDto)
    {
        return new InvoiceDto
        {
            InvoiceNumber = SanitizeString(invoiceDto.InvoiceNumber, 50),
            UniqueSerialNumber = SanitizeString(invoiceDto.UniqueSerialNumber, 100),
            InvoiceDate = SanitizeDate(invoiceDto.InvoiceDate),
            InvoiceType = ValidateEnum<InvoiceType>(invoiceDto.InvoiceType),
            PaymentType = ValidateEnum<InvoicePaymentTypeCode>(invoiceDto.PaymentType),
            Currency = ValidateEnum<CurrencyCode>(invoiceDto.Currency),
            InvoiceNote = SanitizeString(invoiceDto.InvoiceNote, 500),
            Supplier = SanitizeSupplier(invoiceDto.Supplier),
            Customer = SanitizeCustomer(invoiceDto.Customer),
            InvoiceLines = invoiceDto.InvoiceLines?.Select(SanitizeInvoiceLine).ToList() ?? new List<InvoiceLineDto>(),
            Totals = SanitizeTotals(invoiceDto.Totals)
        };
    }

    private static string SanitizeString(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        // Remove potentially dangerous characters
        var sanitized = Regex.Replace(input, @"[<>'""%&;()]", "");
        
        // Trim and limit length
        return sanitized.Trim().Substring(0, Math.Min(sanitized.Length, maxLength));
    }

    private static string SanitizeDate(string dateStr)
    {
        if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            // Ensure date is not in the future
            if (date > DateTime.Today)
            {
                return DateTime.Today.ToString("yyyy-MM-dd");
            }
            return dateStr;
        }
        
        throw new ArgumentException($"Invalid date format: {dateStr}");
    }

    private static T ValidateEnum<T>(string value) where T : struct, Enum
    {
        if (Enum.TryParse<T>(value, out var result))
        {
            return result;
        }
        
        throw new ArgumentException($"Invalid {typeof(T).Name} value: {value}");
    }
}
```

---

## Conclusion

These advanced examples demonstrate:

- ✅ **Complex invoice scenarios** with multiple lines, discounts, and mixed tax categories
- ✅ **Batch processing** with concurrency control and error handling
- ✅ **Web application integration** with dependency injection and telemetry
- ✅ **Comprehensive testing** strategies including performance benchmarks
- ✅ **Monitoring and observability** with custom telemetry and health checks
- ✅ **Security best practices** for credential management and input sanitization

Implement these patterns to build robust, scalable, and secure applications with EFatoraJo SDK.