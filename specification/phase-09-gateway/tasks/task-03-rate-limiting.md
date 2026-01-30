# Task 3: Rate Limiting

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-02 |

## Summary
Add rate limiting middleware with configurable policies.

## Scope
- [x] Add rate limiting middleware using ASP.NET Core built-in rate limiter
- [x] Configure fixed window rate limiting policy (100 requests/60s)
- [x] Apply rate limiting policy to all API routes
- [x] Add Retry-After header to rate limit responses
- [x] Configure rejection response (429 Too Many Requests with ProblemDetails)

## Related Specs
- → [internal-api-communication.md](../../high-level-specs/internal-api-communication.md)

---
## Notes
- X-RateLimit-* headers omitted - Retry-After is sufficient for demo
