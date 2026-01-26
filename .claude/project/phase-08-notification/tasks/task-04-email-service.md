# Task 4: Email Service

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Summary
Implement simulated email service that logs emails instead of actually sending them.

## Scope
- [ ] Create `IEmailService` interface with `SendAsync(EmailMessage)` method
- [ ] Create `EmailMessage` record (To, Subject, HtmlBody)
- [ ] Create `EmailResult` record (Success, ErrorMessage)
- [ ] Implement `SimulatedEmailService` that logs email details
- [ ] Log email recipient, subject, and body (for debugging/verification)
- [ ] Register `IEmailService` in DI container

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 6.4. Consumer Implementation Example)

---
## Notes
(Updated during implementation)
