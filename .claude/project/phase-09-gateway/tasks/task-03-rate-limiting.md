# Task 3: Rate Limiting

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | 🔵 in_progress |
| Dependencies | task-02 |

## Summary
Add rate limiting middleware with configurable policies.

## Scope
- [ ] Add rate limiting middleware using ASP.NET Core built-in rate limiter
- [ ] Configure fixed window rate limiting policy
- [ ] Set up different limits for different route patterns if needed
- [ ] Add rate limit headers to responses (X-RateLimit-*)
- [ ] Configure rejection response (429 Too Many Requests)

## Related Specs
- → [internal-api-communication.md](../../high-level-specs/internal-api-communication.md)

---
## Notes
- X-RateLimit-* headers omitted - Retry-After is sufficient for demo
