# Troubleshooting

Common issues and solutions when working with EShop Demo.

## Docker Issues

### Containers not starting
```bash
# Check Docker is running
docker info

# Check for port conflicts
lsof -i :5432  # PostgreSQL
lsof -i :5672  # RabbitMQ
lsof -i :15672 # RabbitMQ Management

# Clean up old containers
docker compose down -v
docker system prune -f
```

### Out of disk space
```bash
# Remove unused images and volumes
docker system prune -a --volumes

# Check disk usage
docker system df
```

### Container networking issues
```bash
# Restart Docker Desktop (macOS/Windows)
# Or restart Docker daemon (Linux)
sudo systemctl restart docker

# Recreate networks
docker network prune -f
```

## Aspire Startup Problems

### Dashboard not opening
- Check console output for the dashboard URL (port is dynamic)
- Ensure no firewall blocking localhost connections
- Try accessing `https://localhost:<port>` directly

### Services fail to start
```bash
# Check Aspire logs in console output
dotnet run --project src/AppHost

# Run with detailed logging
dotnet run --project src/AppHost --verbosity detailed
```

### Port already in use
```bash
# Find process using the port
lsof -i :<port>

# Kill the process
kill -9 <pid>

# Or use the cleanup script
./tools/e2e-test/kill-services.sh
```

### Services not discovering each other
- Aspire uses environment variables for service discovery
- Check `CONNECTIONSTRINGS__*` and `SERVICES__*` environment variables
- Restart the entire Aspire application (Ctrl+C and re-run)

## Database Issues

### Connection refused
```bash
# Check PostgreSQL container is running
docker ps | grep postgres

# Check connection string in Aspire dashboard
# Navigate to service → Environment Variables
```

### Migration failures
```bash
# Run migrations manually
dotnet ef database update --project src/Services/Order/Order.Infrastructure

# Reset database (development only)
docker compose down -v
dotnet run --project src/AppHost
```

### Database state issues
```bash
# Connect to database directly
docker exec -it <postgres-container> psql -U postgres -d order_db

# Check tables
\dt

# Truncate data (careful!)
TRUNCATE orders CASCADE;
```

## RabbitMQ Issues

### Connection failures
```bash
# Check RabbitMQ container
docker ps | grep rabbitmq

# Access management UI
open http://localhost:15672  # guest/guest

# Check queues and exchanges
```

### Messages not being consumed
- Check consumer service is running (Notification, Analytics)
- Verify queue bindings in RabbitMQ management UI
- Check for poison messages in error queues

### Message stuck in queue
```bash
# Purge queue via management UI or CLI
docker exec <rabbitmq-container> rabbitmqctl purge_queue <queue-name>
```

## Test Issues

### Integration tests failing
```bash
# Ensure Docker is running (Testcontainers requirement)
docker info

# Clean test artifacts
dotnet clean tests/Order.IntegrationTests
dotnet test tests/Order.IntegrationTests --no-build
```

### E2E tests timing out
- E2E tests spin up entire Aspire application - first run is slower
- Increase timeout if needed in test configuration
- Check available system resources (CPU, memory)

### Testcontainers issues
```bash
# Pull images manually
docker pull postgres:17
docker pull rabbitmq:4-management

# Check Testcontainers logs
# Set environment variable for debug output
export TESTCONTAINERS_LOG_LEVEL=DEBUG
```

### Flaky tests
- Check for race conditions in async code
- Ensure proper test isolation (no shared state)
- Use `Respawn` to reset database state between tests

## Debugging Tips

### Enable detailed logging
```csharp
// In appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Trace requests across services
```bash
# Use CorrelationId tracing
./tools/e2e-test/trace-correlation.sh <correlation-id>
```

### Attach debugger to running service
1. Start Aspire: `dotnet run --project src/AppHost`
2. In IDE, attach to process by name (e.g., `Order.API`)
3. Set breakpoints and reproduce the issue

### Check OpenTelemetry traces
- Open Aspire Dashboard
- Navigate to Traces tab
- Filter by service or trace ID
