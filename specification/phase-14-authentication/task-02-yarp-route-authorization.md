# Task 02: YARP Route Authorization

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Summary
Add authorization policies to YARP routes, allowing some routes to be public and others to require authentication.

## Scope
- [ ] Define authorization policies (e.g., "authenticated", "anonymous")
- [ ] Configure YARP routes with AuthorizationPolicy metadata
- [ ] Make health endpoints public (no auth required)
- [ ] Require authentication for API endpoints
- [ ] Test with valid/invalid tokens

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.3 Entra ID App Registrations)

---
## Notes
- YARP supports AuthorizationPolicy per route via metadata
- Health check endpoints (/health, /alive) must remain public
- OpenAPI/Swagger endpoints should be configurable (dev vs prod)
