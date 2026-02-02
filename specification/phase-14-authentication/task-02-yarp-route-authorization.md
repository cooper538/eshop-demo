# Task 02: YARP Route Authorization

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Add authorization policies to YARP routes, allowing some routes to be public and others to require authentication.

## Scope
- [x] Define authorization policies (e.g., "authenticated", "anonymous")
- [x] Configure YARP routes with AuthorizationPolicy metadata
- [x] Make health endpoints public (no auth required)
- [x] Require authentication for API endpoints
- [x] Test with valid/invalid tokens

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.3 Entra ID App Registrations)

---
## Notes
- YARP supports AuthorizationPolicy per route via metadata
- Health check endpoints (/health, /alive) must remain public
- OpenAPI/Swagger endpoints should be configurable (dev vs prod)
