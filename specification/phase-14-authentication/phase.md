# Phase 14: Authentication

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Add JWT Bearer authentication to the API Gateway with YARP route-level authorization. Backend services remain unauthenticated (internal network only).

## Scope
- [x] Configure JWT Bearer authentication in Gateway
- [x] Add authorization policies for YARP routes
- [x] Document Azure AD (Entra ID) setup for token acquisition

## Tasks

| # | ID | Task | Status | Dependencies |
|---|-----|------|--------|--------------|
| 1 | task-01 | Gateway JWT Bearer Setup | ✅ completed | - |
| 2 | task-02 | YARP Route Authorization | ✅ completed | task-01 |
| 3 | task-03 | Azure AD Setup Documentation | ✅ completed | - |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.3 Entra ID App Registrations)

---
## Notes
- Backend services do NOT implement auth - only Gateway validates tokens
- Services trust requests from Gateway (internal network)
- Minimal scope: JWT validation + route policies only

### Security Hardening
- Algorithm whitelist: Only RS256 (prevents algorithm confusion attacks)
- Explicit token validation parameters
- Security event logging for SIEM integration
- HSTS enabled in production (365 days)

### Security Considerations (Demo Limitations)
| Aspect | Current State | Production Recommendation |
|--------|---------------|---------------------------|
| Backend auth | None (trusts Gateway) | Add mTLS or internal tokens |
| Rate limiting | Not implemented | Add rate limiting middleware |
| Network isolation | Container Apps internal | Add VNet + Private Endpoints |
| Swagger access | Configurable per env | Disable in production |

> **Note:** This is a demo project. For production, implement defense-in-depth with multiple security layers.
