# Implementation Notes

Brief implementation notes. Format: `[XX-YY] task name - note`

---

- [01-06] EShop.ServiceClients - Configure ServiceClients section in appsettings.json when services are created
- [01-06] EShop.ServiceClients - Add IValidateOptions for ServiceClientOptions validation (URLs, timeout > 0, etc.)
- [03-02] CI - Add `dotnet format --verify-no-changes` to CI pipeline for code style enforcement
- [03-04] Restructure EShop.Common by layers (Application, Infrastructure, API)
- [04-02] Order Service - Move 15min stockReservation TTL to configuration/settings
- [04-02] Order Service - Add IDateTimeProvider before writing tests
- [05-03] Aspire DB naming - Consider configuration for DB resource names ("orderdb", "productdb") between AppHost and services
- [08-06] StockLowConsumer - Move AdminEmail from hardcoded constant to appsettings.json or environment variable
