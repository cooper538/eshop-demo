# Implementation Notes

Brief implementation notes. Format: `[XX-YY] task name - note`

---

- [01-05] EShop.Common - Missing tests, add later
- [01-06] EShop.ServiceClients - Missing tests (GrpcProductServiceClient, HttpProductServiceClient, handlers, interceptors)
- [01-06] EShop.ServiceClients - Configure ServiceClients section in appsettings.json when services are created
- [01-06] EShop.ServiceClients - Add IValidateOptions for ServiceClientOptions validation (URLs, timeout > 0, etc.)
- [03-02] CI - Add `dotnet format --verify-no-changes` to CI pipeline for code style enforcement
- [03-04] Create generic pagination extension for EF + Application layer + API layer
- [03-04] Restructure EShop.Common by layers (Application, Infrastructure, API)
