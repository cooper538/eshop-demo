# Troubleshooting

Common issues with Aspire, gRPC, and RabbitMQ.

## Aspire

### Services not starting
```bash
# Check console output for errors
dotnet run --project src/AppHost

# Verbose logging
dotnet run --project src/AppHost --verbosity detailed
```

### Port already in use
```bash
lsof -i :<port>
kill -9 <pid>
```

### Services not discovering each other
- Aspire injects connection strings via environment variables
- Check `CONNECTIONSTRINGS__*` in Aspire Dashboard → Service → Environment
- Restart entire Aspire app (Ctrl+C and re-run)

### Dashboard URL
Port is dynamic - check console output for the actual URL.

## gRPC

### Connection refused between services
- Verify service is registered in AppHost with `.WithReference()`
- Check gRPC endpoint is exposed: `.WithHttpsEndpoint(port: 5051, name: "grpc")`
- Use Aspire Dashboard traces to see gRPC call failures

### SSL/TLS errors
Development uses self-signed certificates. If issues persist:
```csharp
// In development only - trust all certificates
.ConfigureChannel(o => o.HttpHandler = new SocketsHttpHandler
{
    SslOptions = new SslClientAuthenticationOptions
    {
        RemoteCertificateValidationCallback = (_, _, _, _) => true
    }
});
```

### Debugging gRPC calls
- Aspire Dashboard → Traces shows full gRPC request/response
- Filter by service name or CorrelationId

## RabbitMQ

### Connection failures
```bash
# Check container is running
docker ps | grep rabbitmq

# Access management UI (guest/guest)
open http://localhost:15672
```

### Messages not consumed
- Verify consumer service is running (check Aspire Dashboard)
- Check queue bindings in RabbitMQ Management UI
- Look for poison messages in `_error` queues

### Purge stuck messages
```bash
docker exec <rabbitmq-container> rabbitmqctl purge_queue <queue-name>
```

## Quick Debug

| What | Where |
|------|-------|
| Service logs | Aspire Dashboard → Logs |
| Distributed traces | Aspire Dashboard → Traces |
| Request tracing | `./tools/e2e-test/trace-correlation.sh <id>` |
| RabbitMQ queues | http://localhost:15672 |
