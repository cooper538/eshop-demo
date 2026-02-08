# Task 02: gRPC Interceptors Streaming Support

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ⬜ pending |
| Dependencies | - |

## Summary
Add server-streaming overrides to all gRPC interceptors (4 server-side, 2 client-side) so streaming RPCs get the same cross-cutting behavior as unary calls.

## Scope
- [ ] Add `ServerStreamingServerHandler` override to `CorrelationIdServerInterceptor` -- wrap stream in correlation scope
- [ ] Add `ServerStreamingServerHandler` override to `GrpcLoggingInterceptor` -- stopwatch logging around stream lifecycle
- [ ] Add `ServerStreamingServerHandler` override to `GrpcExceptionInterceptor` -- exception-to-`RpcException` mapping
- [ ] Add `ServerStreamingServerHandler` override to `GrpcValidationInterceptor` -- FluentValidation before stream starts
- [ ] Server interceptor files: `src/Common/EShop.Common.Api/Grpc/` (4 files)
- [ ] Add `AsyncServerStreamingCall` override to `CorrelationIdClientInterceptor` -- attach correlation header
- [ ] Add `AsyncServerStreamingCall` override to `LoggingInterceptor` -- log stream call
- [ ] Client interceptor files: `src/Common/EShop.ServiceClients/Infrastructure/Grpc/` (2 files)

## Related Specs
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Interceptors, CorrelationId Propagation)

---
## Notes
(Updated during implementation)
