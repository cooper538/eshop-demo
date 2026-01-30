# Task 02: Resilience Configuration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Configure resilience policies for gRPC client calls using gRPC built-in retry mechanism.

## Scope
- [x] Configure gRPC built-in retry policy with exponential backoff
- [x] Set retryable status codes (Unavailable, DeadlineExceeded, Aborted)
- [x] Configure retry parameters (max attempts, backoff, jitter)
- [x] Add logging interceptor for gRPC call tracking

## Implementation

### Retry Policy Configuration
```csharp
// ServiceCollectionExtensions.cs - CreateServiceConfig()
var retryPolicy = new RetryPolicy
{
    MaxAttempts = retryOptions.MaxRetryCount + 1,  // includes initial attempt
    InitialBackoff = TimeSpan.FromMilliseconds(retryOptions.BaseDelayMs),
    MaxBackoff = TimeSpan.FromMilliseconds(retryOptions.MaxBackoffMs),
    BackoffMultiplier = retryOptions.BackoffMultiplier,
    RetryableStatusCodes = { Unavailable, DeadlineExceeded, Aborted }
};
```

### Default Configuration (ResilienceOptions)
| Parameter | Default Value |
|-----------|---------------|
| MaxRetryCount | 3 |
| BaseDelayMs | 100 |
| MaxBackoffMs | 5000 |
| BackoffMultiplier | 2 |

### Key Files
- `src/Common/EShop.ServiceClients/Extensions/ServiceCollectionExtensions.cs`
- `src/Common/EShop.ServiceClients/Configuration/ResilienceOptions.cs`
- `src/Common/EShop.ServiceClients/Infrastructure/Grpc/LoggingInterceptor.cs`

## Design Decision
Used gRPC built-in retry instead of Polly circuit breaker:
- Simpler configuration (no additional library)
- Native integration with gRPC channel
- Sufficient for current use case (single downstream service)
- Automatic jitter included

## Related Specs
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Resilience Policies)

---
## Notes
Circuit breaker pattern can be added later via Polly if needed for more complex scenarios.
