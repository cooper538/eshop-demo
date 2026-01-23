# Error Handling

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | HTTP API error handling |
| Standard | RFC 7807 (ProblemDetails) |
| Applies To | All HTTP APIs (External + Internal REST) |

---

## 1. HTTP Status Codes

We use a minimal set of status codes:

| Code | Name | When to Use |
|------|------|-------------|
| **200** | OK | Successful GET, PUT, DELETE |
| **201** | Created | Successful POST (resource created) |
| **400** | Bad Request | Validation errors, malformed request |
| **404** | Not Found | Resource doesn't exist |
| **409** | Conflict | Optimistic concurrency failure, duplicate |
| **500** | Internal Server Error | Unexpected server errors |

### 1.1 What We Don't Use (and why)

| Code | Reason |
|------|--------|
| 401/403 | Auth is out of scope for this demo |
| 422 | Validation goes to 400 (simpler) |
| 503 | Client-side Polly handles service unavailability |

---

## 2. Response Format: ProblemDetails

All error responses use **RFC 7807 ProblemDetails** format.

### 2.1 Standard Fields

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/products"
}
```

| Field | Description |
|-------|-------------|
| `type` | URI reference identifying the problem type |
| `title` | Short human-readable summary |
| `status` | HTTP status code |
| `detail` | Human-readable explanation |
| `instance` | URI of the specific occurrence |

### 2.2 Validation Errors (400)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "Name": ["Name is required."],
    "Price": ["Price must be greater than 0."]
  }
}
```

### 2.3 Not Found (404)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Product with ID '550e8400-e29b-41d4-a716-446655440000' was not found."
}
```

### 2.4 Conflict (409)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "The resource was modified by another user. Please refresh and try again."
}
```

### 2.5 Server Error (500)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred.",
  "extensions": {
    "correlationId": "abc-123-def-456"
  }
}
```

> **Note**: `correlationId` is included **only for 5xx errors** to help with debugging. Client errors (4xx) don't include it.

---

## 3. Exception Hierarchy

Defined in `EShop.Common/Exceptions/`:

```csharp
// Base exception for all domain/application exceptions
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message) { }
}

public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message) { }

    public static NotFoundException For<T>(Guid id)
        => new($"{typeof(T).Name} with ID '{id}' was not found.");
}

public sealed class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

public sealed class ConflictException : ApplicationException
{
    public ConflictException(string message) : base(message) { }
}
```

---

## 4. Exception → HTTP Mapping

| Exception | HTTP Status | Title |
|-----------|-------------|-------|
| `NotFoundException` | 404 | Not Found |
| `ValidationException` | 400 | Bad Request |
| `ConflictException` | 409 | Conflict |
| Any other exception | 500 | Internal Server Error |

---

## 5. Implementation

### 5.1 Setup in Program.cs

```csharp
// Program.cs
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();
```

### 5.2 Global Exception Handler

```csharp
// EShop.Common/Middleware/GlobalExceptionHandler.cs
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ICorrelationIdAccessor _correlationContext;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ICorrelationIdAccessor correlationContext,
        ILogger<GlobalExceptionHandler> logger)
    {
        _correlationContext = correlationContext;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ValidationException => (StatusCodes.Status400BadRequest, "Bad Request"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        // Log server errors
        if (statusCode >= 500)
        {
            _logger.LogError(exception,
                "Unhandled exception. CorrelationId: {CorrelationId}",
                _correlationContext.CorrelationId);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        // Add validation errors if applicable
        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        // Add correlationId only for 5xx errors
        if (statusCode >= 500)
        {
            problemDetails.Extensions["correlationId"] = _correlationContext.CorrelationId;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

### 5.3 DI Registration Extension

```csharp
// EShop.Common/Extensions/ErrorHandlingExtensions.cs
public static class ErrorHandlingExtensions
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
```

---

## 6. Usage in Services

### 6.1 Throwing Exceptions

```csharp
// In handler
public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
{
    var product = await _db.Products.FindAsync(request.Id, ct);

    if (product is null)
        throw NotFoundException.For<Product>(request.Id);

    return new ProductDto(product);
}
```

### 6.2 Service Registration

```csharp
// Product.API/Program.cs
builder.Services.AddErrorHandling();

var app = builder.Build();

app.UseErrorHandling();
```

---

## 7. Testing Error Responses

```csharp
[Fact]
public async Task GetProduct_WhenNotFound_Returns404WithProblemDetails()
{
    // Arrange
    var nonExistentId = Guid.NewGuid();

    // Act
    var response = await _client.GetAsync($"/api/products/{nonExistentId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);

    var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problem!.Status.Should().Be(404);
    problem.Title.Should().Be("Not Found");
    problem.Detail.Should().Contain(nonExistentId.ToString());
}
```

---

## Related Documents

- [Correlation ID Flow](./correlation-id-flow.md) - CorrelationId in error responses
- [Shared Projects](./shared-projects.md) - Exception classes location
- [Unit Testing](./unit-testing.md) - Testing patterns
