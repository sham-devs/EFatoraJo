# EFatoraJo Configuration Guide

## Overview

This comprehensive guide covers all aspects of configuring EFatoraJo SDK and Console Application for different environments, security requirements, and operational settings.

---

## Configuration Options

### 1. Environment Variables

#### Production Environment Variables

| Variable | Description | Required | Example |
|----------|-------------|-----------|---------|
| `EFATORA_CLIENT_ID` | API client ID from Jofotara | Yes | `your-client-id-here` |
| `EFATORA_SECRET_KEY` | API secret key from Jofotara | Yes | `your-secret-key-here` |
| `EFATORA_TAX_NUMBER` | Supplier tax/VAT number | Yes | `1234567890` |
| `EFATORA_ACTIVITY_CODE` | Income source/activity code | Yes | `62010` |
| `EFATORA_SUPPLIER_NAME` | Registered supplier name | Yes | `Your Company Name` |
| `EFATORA_TIMEOUT_SECONDS` | API request timeout | No | `30` |
| `EFATORA_MAX_RETRIES` | Maximum retry attempts | No | `3` |
| `EFATORA_LOG_LEVEL` | Logging level | No | `Information` |
| `EFATORA_BASE_URL` | Custom API base URL | No | `https://backend.jofotara.gov.jo/core/invoices/` |

#### Setting Environment Variables

**Windows (PowerShell):**
```powershell
# Session-level (current terminal session only)
$env:EFATORA_CLIENT_ID = "your-client-id"
$env:EFATORA_SECRET_KEY = "your-secret-key"

# System-level (persistent)
[System.Environment]::SetEnvironmentVariable("EFATORA_CLIENT_ID", "your-client-id", "Machine")
[System.Environment]::SetEnvironmentVariable("EFATORA_SECRET_KEY", "your-secret-key", "Machine")
```

**Windows (Command Prompt):**
```cmd
# Session-level
set EFATORA_CLIENT_ID=your-client-id
set EFATORA_SECRET_KEY=your-secret-key

# System-level (requires administrator privileges)
setx EFATORA_CLIENT_ID "your-client-id" /M
setx EFATORA_SECRET_KEY "your-secret-key" /M
```

**Linux/macOS (Bash):**
```bash
# Session-level
export EFATORA_CLIENT_ID="your-client-id"
export EFATORA_SECRET_KEY="your-secret-key"

# Persistent (add to ~/.bashrc or ~/.bash_profile)
echo 'export EFATORA_CLIENT_ID="your-client-id"' >> ~/.bashrc
echo 'export EFATORA_SECRET_KEY="your-secret-key"' >> ~/.bashrc
source ~/.bashrc
```

### 2. .NET User Secrets (Development Only)

#### Setting User Secrets

```bash
# Navigate to project directory
cd /path/to/EFatoraJo/

# Set SDK credentials
dotnet user-secrets set "ClientId" "your-client-id"
dotnet user-secrets set "SecretKey" "your-secret-key"

# Set supplier information
dotnet user-secrets set "Supplier:TaxVATNumber" "1234567890"
dotnet user-secrets set "Supplier:IncomeSourceSequence" "62010"
dotnet user-secrets set "Supplier:RegisteredSupplierName" "Your Company Name"

# Set optional configuration
dotnet user-secrets set "TimeoutSeconds" "30"
dotnet user-secrets set "MaxRetries" "3"
dotnet user-secrets set "LogLevel" "Information"
```

#### Listing User Secrets

```bash
# List all secrets
dotnet user-secrets list

# List specific secret
dotnet user-secrets get "ClientId"
dotnet user-secrets get "Supplier:TaxVATNumber"
```

#### Removing User Secrets

```bash
# Remove specific secret
dotnet user-secrets remove "ClientId"
dotnet user-secrets remove "Supplier:TaxVATNumber"

# Clear all secrets
dotnet user-secrets clear
```

### 3. Configuration Files

#### appsettings.json

```json
{
  "EFatora": {
    "ClientId": "your-client-id",
    "SecretKey": "your-secret-key",
    "Supplier": {
      "TaxVATNumber": "1234567890",
      "IncomeSourceSequence": "62010",
      "RegisteredSupplierName": "Your Company Name"
    },
    "ApiSettings": {
      "BaseUrl": "https://backend.jofotara.gov.jo/core/invoices/",
      "TimeoutSeconds": 30,
      "MaxRetries": 3,
      "RetryDelaySeconds": 1
    },
    "Logging": {
      "LogLevel": "Information",
      "EnableRequestLogging": true,
      "EnableResponseLogging": false
    },
    "Validation": {
      "StrictMode": true,
      "AllowFutureDates": false,
      "ValidateChecksums": true
    }
  }
}
```

#### appsettings.Development.json

```json
{
  "EFatora": {
    "ClientId": "dev-client-id",
    "SecretKey": "dev-secret-key",
    "ApiSettings": {
      "BaseUrl": "https://test-backend.jofotara.gov.jo/core/invoices/",
      "TimeoutSeconds": 60,
      "MaxRetries": 5
    },
    "Logging": {
      "LogLevel": "Debug",
      "EnableRequestLogging": true,
      "EnableResponseLogging": true
    }
  }
}
```

#### appsettings.Production.json

```json
{
  "EFatora": {
    "ApiSettings": {
      "BaseUrl": "https://backend.jofotara.gov.jo/core/invoices/",
      "TimeoutSeconds": 30,
      "MaxRetries": 3
    },
    "Logging": {
      "LogLevel": "Warning",
      "EnableRequestLogging": false,
      "EnableResponseLogging": false
    },
    "Validation": {
      "StrictMode": true,
      "AllowFutureDates": false
    }
  }
}
```

---

## Environment-Specific Configuration

### 1. Development Environment

#### Configuration Setup

```csharp
// Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add configuration
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(builder.Environment.IsDevelopment());

        // Configure services
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();
        Configure(app, app.Environment);
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure EFatoraJo options
        services.Configure<EFatoraOptions>(configuration.GetSection("EFatora"));
        
        // Register EFatoraJo service
        services.AddTransient<IEFatoraJoService, EFatoraJoService>();
        
        // Configure logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }
}
```

#### Development Configuration Options

| Setting | Recommended Value | Description |
|----------|-------------------|-------------|
| `LogLevel` | `Debug` | Detailed logging for development |
| `EnableRequestLogging` | `true` | Log all API requests |
| `EnableResponseLogging` | `true` | Log all API responses |
| `BaseUrl` | Test environment URL | Use test API endpoints |
| `TimeoutSeconds` | `60` | Longer timeout for debugging |
| `MaxRetries` | `5` | More retries for unstable test environment |

### 2. Staging Environment

#### Configuration Setup

```json
{
  "EFatora": {
    "ApiSettings": {
      "BaseUrl": "https://staging-backend.jofotara.gov.jo/core/invoices/",
      "TimeoutSeconds": 45,
      "MaxRetries": 3,
      "RetryDelaySeconds": 2
    },
    "Logging": {
      "LogLevel": "Information",
      "EnableRequestLogging": true,
      "EnableResponseLogging": false
    },
    "Validation": {
      "StrictMode": true,
      "AllowFutureDates": false
    }
  }
}
```

### 3. Production Environment

#### Configuration Setup

```json
{
  "EFatora": {
    "ApiSettings": {
      "BaseUrl": "https://backend.jofotara.gov.jo/core/invoices/",
      "TimeoutSeconds": 30,
      "MaxRetries": 3,
      "RetryDelaySeconds": 1
    },
    "Logging": {
      "LogLevel": "Warning",
      "EnableRequestLogging": false,
      "EnableResponseLogging": false
    },
    "Validation": {
      "StrictMode": true,
      "AllowFutureDates": false,
      "ValidateChecksums": true
    }
  }
}
```

---

## Security Configuration

### 1. Credential Protection

#### Best Practices

```csharp
public class SecureConfigurationManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecureConfigurationManager> _logger;

    public SecureConfigurationManager(IConfiguration configuration, ILogger<SecureConfigurationManager> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public EFatoraCredentials GetCredentials()
    {
        // Priority 1: Environment variables (most secure)
        var credentials = GetFromEnvironment();
        if (credentials != null) return credentials;

        // Priority 2: Azure Key Vault (cloud secure storage)
        credentials = GetFromKeyVault();
        if (credentials != null) return credentials;

        // Priority 3: User secrets (development only)
        if (IsDevelopmentEnvironment())
        {
            credentials = GetFromUserSecrets();
            if (credentials != null) return credentials;
        }

        throw new InvalidOperationException("No valid credential source found");
    }

    private EFatoraCredentials GetFromEnvironment()
    {
        var clientId = Environment.GetEnvironmentVariable("EFATORA_CLIENT_ID");
        var secretKey = Environment.GetEnvironmentVariable("EFATORA_SECRET_KEY");
        var taxNumber = Environment.GetEnvironmentVariable("EFATORA_TAX_NUMBER");
        var activityCode = Environment.GetEnvironmentVariable("EFATORA_ACTIVITY_CODE");
        var supplierName = Environment.GetEnvironmentVariable("EFATORA_SUPPLIER_NAME");

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(secretKey))
        {
            return new EFatoraCredentials
            {
                ClientId = clientId,
                SecretKey = secretKey,
                TaxNumber = taxNumber,
                ActivityCode = activityCode,
                SupplierName = supplierName
            };
        }

        return null;
    }

    private EFatoraCredentials GetFromKeyVault()
    {
        try
        {
            var keyVaultUri = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URI");
            if (string.IsNullOrEmpty(keyVaultUri)) return null;

            // Azure Key Vault implementation
            var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            
            var clientIdSecret = await secretClient.GetSecretAsync("EFATORA-CLIENT-ID");
            var secretKeySecret = await secretClient.GetSecretAsync("EFATORA-SECRET-KEY");
            var taxNumberSecret = await secretClient.GetSecretAsync("EFATORA-TAX-NUMBER");
            var activityCodeSecret = await secretClient.GetSecretAsync("EFATORA-ACTIVITY-CODE");
            var supplierNameSecret = await secretClient.GetSecretAsync("EFATORA-SUPPLIER-NAME");

            return new EFatoraCredentials
            {
                ClientId = clientIdSecret.Value,
                SecretKey = secretKeySecret.Value,
                TaxNumber = taxNumberSecret.Value,
                ActivityCode = activityCodeSecret.Value,
                SupplierName = supplierNameSecret.Value
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve credentials from Azure Key Vault");
            return null;
        }
    }

    private EFatoraCredentials GetFromUserSecrets()
    {
        return new EFatoraCredentials
        {
            ClientId = _configuration["EFatora:ClientId"],
            SecretKey = _configuration["EFatora:SecretKey"],
            TaxNumber = _configuration["EFatora:Supplier:TaxVATNumber"],
            ActivityCode = _configuration["EFatora:Supplier:IncomeSourceSequence"],
            SupplierName = _configuration["EFatora:Supplier:RegisteredSupplierName"]
        };
    }

    private bool IsDevelopmentEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
```

### 2. Encryption Configuration

#### Encrypting Configuration Sections

```json
{
  "EFatora": {
    "ClientId": "AQIDBAUGBlndLWhjaWZ0IHRlc3RlcHQgY2xpZW50IElk", // Encrypted
    "SecretKey": "AQIDBAUGBlndLWhjaWZ0IHRlc3RlcHQgY2xpZW50IElk", // Encrypted
    "UseEncryption": true,
    "EncryptionCertificate": {
      "Thumbprint": "A1B2C3D4E5F6...",
      "StoreLocation": "LocalMachine",
      "StoreName": "My"
    }
  }
}
```

#### Decryption Service

```csharp
public class ConfigurationEncryptionService
{
    private readonly ILogger<ConfigurationEncryptionService> _logger;

    public ConfigurationEncryptionService(ILogger<ConfigurationEncryptionService> logger)
    {
        _logger = logger;
    }

    public string DecryptValue(string encryptedValue, EncryptionConfiguration config)
    {
        try
        {
            var certificate = FindCertificate(config.Thumbprint, config.StoreLocation, config.StoreName);
            
            using var rsa = certificate.GetRSAPrivateKey();
            var encryptedBytes = Convert.FromBase64String(encryptedValue);
            var decryptedBytes = rsa.Decrypt(encryptedBytes, false);
            
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt configuration value");
            throw new InvalidOperationException("Configuration decryption failed", ex);
        }
    }

    private X509Certificate2 FindCertificate(string thumbprint, StoreLocation location, StoreName name)
    {
        var store = new X509Store(name, location);
        store.Open(OpenFlags.ReadOnly);
        
        var certificates = store.Certificates.Find(
            X509FindType.FindByThumbprint,
            thumbprint,
            false);
        
        store.Close();
        return certificates?.Count > 0 ? certificates[0] : null;
    }
}
```

---

## Docker Configuration

### 1. Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Copy csproj and restore
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the files
COPY . .

# Build and publish
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=base /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "EFatoraJo.WebApp.dll"]
```

### 2. Docker Compose

```yaml
version: '3.8'

services:
  efatorajo-app:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - EFATORA_CLIENT_ID=${EFATORA_CLIENT_ID}
      - EFATORA_SECRET_KEY=${EFATORA_SECRET_KEY}
      - EFATORA_TAX_NUMBER=${EFATORA_TAX_NUMBER}
      - EFATORA_ACTIVITY_CODE=${EFATORA_ACTIVITY_CODE}
      - EFATORA_SUPPLIER_NAME=${EFATORA_SUPPLIER_NAME}
    volumes:
      - ./logs:/app/logs
      - ./data:/app/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  efatorajo-processor:
    build:
      context: .
      dockerfile: Dockerfile.processor
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - EFATORA_CLIENT_ID=${EFATORA_CLIENT_ID}
      - EFATORA_SECRET_KEY=${EFATORA_SECRET_KEY}
      - EFATORA_TAX_NUMBER=${EFATORA_TAX_NUMBER}
      - EFATORA_ACTIVITY_CODE=${EFATORA_ACTIVITY_CODE}
      - EFATORA_SUPPLIER_NAME=${EFATORA_SUPPLIER_NAME}
    volumes:
      - ./invoices:/app/invoices
      - ./logs:/app/logs
    restart: unless-stopped
    command: ["dotnet", "EFatoraJo.Processor.dll"]
```

### 3. Environment File (.env)

```bash
# .env file (never commit to version control)
EFATORA_CLIENT_ID=your-production-client-id
EFATORA_SECRET_KEY=your-production-secret-key
EFATORA_TAX_NUMBER=1234567890
EFATORA_ACTIVITY_CODE=62010
EFATORA_SUPPLIER_NAME=Your Company Name

# Optional settings
EFATORA_TIMEOUT_SECONDS=30
EFATORA_MAX_RETRIES=3
EFATORA_LOG_LEVEL=Information
```

---

## Kubernetes Configuration

### 1. ConfigMap

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: efatorajo-config
  namespace: efatorajo
data:
  appsettings.json: |
    {
      "EFatora": {
        "ApiSettings": {
          "BaseUrl": "https://backend.jofotara.gov.jo/core/invoices/",
          "TimeoutSeconds": 30,
          "MaxRetries": 3
        },
        "Logging": {
          "LogLevel": "Information",
          "EnableRequestLogging": false,
          "EnableResponseLogging": false
        }
      }
    }
```

### 2. Secret

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: efatorajo-secrets
  namespace: efatorajo
type: Opaque
data:
  # Base64 encoded values
  EFATORA_CLIENT_ID: eW91ci1jbGllbnQtaWQ= # base64 for "your-client-id"
  EFATORA_SECRET_KEY: eW91ci1zZWNyZXQta2V5 # base64 for "your-secret-key"
  EFATORA_TAX_NUMBER: MTIzNDU2Nzg5MA== # base64 for "1234567890"
  EFATORA_ACTIVITY_CODE: NjIwMTA= # base64 for "62010"
  EFATORA_SUPPLIER_NAME: WW91ciBDb21wYW55IE5hbWU= # base64 for "Your Company Name"
```

### 3. Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: efatorajo-app
  namespace: efatorajo
spec:
  replicas: 3
  selector:
    matchLabels:
      app: efatorajo-app
  template:
    metadata:
      labels:
        app: efatorajo-app
    spec:
      containers:
      - name: efatorajo-app
        image: efatorajo:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        envFrom:
        - configMapRef:
            name: efatorajo-config
        - secretRef:
            name: efatorajo-secrets
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
```

---

## Performance Tuning

### 1. Connection Pooling

```csharp
public class EFatoraJoHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EFatoraOptions _options;

    public EFatoraJoHttpClientFactory(IHttpClientFactory httpClientFactory, IOptions<EFatoraOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("EFatoraJo");
        
        // Configure timeout
        client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        
        // Configure base address
        client.BaseAddress = new Uri(_options.BaseUrl);
        
        // Configure default headers
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "EFatoraJo/1.0.0");
        
        return client;
    }
}

// In Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient("EFatoraJo", client =>
    {
        client.BaseAddress = new Uri("https://backend.jofotara.gov.jo/core/invoices/");
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    
    services.AddTransient<EFatoraJoHttpClientFactory>();
}
```

### 2. Caching Configuration

```csharp
public class EFatoraJoCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly EFatoraOptions _options;

    public EFatoraJoCacheService(IMemoryCache cache, IOptions<EFatoraOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await _cache.GetAsync<T>(key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = expiration ?? TimeSpan.FromHours(1),
            Priority = CacheItemPriority.Normal,
            Size = 1
        };

        await _cache.SetAsync(key, value, options);
    }

    public async Task RemoveAsync(string key)
    {
        _cache.Remove(key);
    }
}

// In Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMemoryCache(options =>
    {
        options.SizeLimit = 1000; // Max 1000 items
        options.CompactionPercentage = 0.05; // Compact when 5% full
        options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
    });
    
    services.AddTransient<ICacheService, EFatoraJoCacheService>();
}
```

---

## Monitoring Configuration

### 1. Application Insights

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-instrumentation-key",
    "InstrumentationKey": "your-instrumentation-key"
  },
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### 2. Serilog Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/efatorajo-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "instrumentationKey": "your-instrumentation-key",
          "telemetryConverter": "Serilog.TelemetryConverters.TraceTelemetryConverter, Serilog"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

---

## Troubleshooting Configuration Issues

### 1. Common Configuration Problems

| Issue | Symptoms | Solution |
|--------|-----------|----------|
| Missing credentials | Authentication errors (Exit Code 3) | Verify environment variables or user secrets are properly set |
| Invalid timeout | Requests timing out | Check TimeoutSeconds setting, ensure reasonable value |
| Wrong environment | Using test API in production | Verify ASPNETCORE_ENVIRONMENT variable |
| Missing configuration | Application fails to start | Check appsettings.json exists and is valid JSON |
| Permission denied | Cannot access configuration files | Verify file permissions and user access rights |

### 2. Configuration Validation

```csharp
public class ConfigurationValidator
{
    public static ValidationResult ValidateConfiguration(IConfiguration configuration)
    {
        var errors = new List<string>();

        // Validate required settings
        var clientId = configuration["EFatora:ClientId"];
        if (string.IsNullOrEmpty(clientId))
        {
            errors.Add("EFatora:ClientId is required");
        }

        var secretKey = configuration["EFatora:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            errors.Add("EFatora:SecretKey is required");
        }

        // Validate optional settings
        if (int.TryParse(configuration["EFatora:TimeoutSeconds"], out var timeout))
        {
            if (timeout < 1 || timeout > 300)
            {
                errors.Add("EFatora:TimeoutSeconds must be between 1 and 300 seconds");
            }
        }

        if (int.TryParse(configuration["EFatora:MaxRetries"], out var retries))
        {
            if (retries < 0 || retries > 10)
            {
                errors.Add("EFatora:MaxRetries must be between 0 and 10");
            }
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}
```

### 3. Configuration Debugging

```csharp
public class ConfigurationDebugger
{
    public static void LogConfiguration(IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("=== EFatoraJo Configuration ===");
        
        // Log all EFatora settings (without sensitive data)
        var section = configuration.GetSection("EFatora");
        foreach (var child in section.GetChildren())
        {
            if (child.Path.Contains("SecretKey") || child.Path.Contains("ClientId"))
            {
                logger.LogInformation("{Key}: [REDACTED]", child.Path);
            }
            else
            {
                logger.LogInformation("{Key}: {Value}", child.Path, child.Value);
            }
        }
        
        // Log environment
        var env = configuration["ASPNETCORE_ENVIRONMENT"];
        logger.LogInformation("Environment: {Environment}", env);
        
        // Log connection info
        var baseUrl = configuration["EFatora:ApiSettings:BaseUrl"];
        logger.LogInformation("API Base URL: {BaseUrl}", baseUrl);
        
        logger.LogInformation("=== End Configuration ===");
    }
}
```

---

## Best Practices

### 1. Configuration Management

- ✅ **Use environment variables** for production credentials
- ✅ **Use user secrets** for development credentials
- ✅ **Never commit** sensitive configuration to version control
- ✅ **Use different configurations** for different environments
- ✅ **Validate configuration** at application startup
- ✅ **Use secure storage** for sensitive data (Azure Key Vault, etc.)

### 2. Security Considerations

- ✅ **Encrypt sensitive configuration** when using files
- ✅ **Restrict file permissions** on configuration files
- ✅ **Use managed identities** in cloud environments
- ✅ **Rotate credentials** regularly
- ✅ **Audit configuration changes**
- ✅ **Use separate service accounts** for different environments

### 3. Performance Optimization

- ✅ **Configure appropriate timeouts** for your network conditions
- ✅ **Use connection pooling** for HTTP clients
- ✅ **Implement caching** for frequently accessed data
- ✅ **Monitor configuration** changes and reloads
- ✅ **Use health checks** to verify configuration validity

---

## Conclusion

This configuration guide provides:

- ✅ **Comprehensive configuration options** for all environments
- ✅ **Security best practices** for credential management
- ✅ **Container and orchestration** configurations
- ✅ **Performance tuning** guidelines
- ✅ **Troubleshooting** assistance for common issues

Proper configuration is essential for reliable and secure operation of EFatoraJo applications in production environments.