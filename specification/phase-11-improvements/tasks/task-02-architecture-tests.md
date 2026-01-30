# Task 02: Architecture Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Architecture tests using NetArchTest.Rules ensuring Clean Architecture and DDD compliance across the entire project.

## Motivation

**Proč architektonické testy?**

1. **Automatická kontrola** - CI/CD pipeline automaticky odhalí porušení architektury
2. **Dokumentace** - Testy slouží jako živá dokumentace architektonických pravidel
3. **Prevence regresí** - Nový kód nemůže porušit zavedená pravidla
4. **Onboarding** - Noví vývojáři pochopí architekturu z testů

## Zvolená knihovna: NetArchTest.Rules

**Důvody volby:**
- Jednodušší fluent API oproti ArchUnitNET
- Lepší integrace s MSTest (primární testovací framework v projektu)
- Aktivnější .NET komunita a více příkladů
- Rychlejší načítání assembly

## Testovací projekt

**Umístění:** `tests/EShop.ArchitectureTests/`

**Struktura:**
```
tests/EShop.ArchitectureTests/
├── EShop.ArchitectureTests.csproj
├── GlobalUsings.cs
├── TestBase/
│   ├── ArchitectureTestBase.cs      # Assembly references
│   └── ServiceLayerDefinitions.cs   # Namespace patterns
├── CleanArchitecture/
│   ├── DependencyTests.cs           # Layer dependency rules
│   └── LayerIsolationTests.cs       # Controllers/DbContext placement
├── DDD/
│   ├── EntityTests.cs               # Entity inheritance
│   └── DomainEventTests.cs          # Domain event rules
├── CQRS/
│   ├── CommandTests.cs              # ICommand implementation
│   ├── QueryTests.cs                # IQuery implementation
│   ├── HandlerTests.cs              # Handler rules
│   └── ValidatorTests.cs            # Validator rules
└── NamingConventions/
    ├── EntityNamingTests.cs         # *Entity suffix
    └── CqrsNamingTests.cs           # Command/Query/Handler suffixes
```

## Testovaná pravidla

### 1. Clean Architecture - Dependency Rules

| Vrstva | Může záviset na | Nesmí záviset na |
|--------|-----------------|------------------|
| Domain | SharedKernel | Application, Infrastructure, API, Common, Contracts, ServiceClients |
| Application | Domain, SharedKernel, Common, Contracts | Infrastructure, API, **ServiceClients** |
| Infrastructure | Domain, Application, Common, ServiceClients | API |
| API | Všechny vrstvy | - |

**Důležité:** Application vrstva nesmí záviset na ServiceClients - to je implementační detail patřící do Infrastructure. Application definuje interfaces (v Contracts), Infrastructure je implementuje pomocí ServiceClients.

### 2. Cross-Service Isolation

- `Order.Domain` nesmí přímo referencovat `Products.Domain` (a naopak)
- `Order.Application` nesmí přímo referencovat `Products.Application` (a naopak)
- Služby komunikují přes:
  - **Contracts** - integration events, shared DTOs, interfaces
  - **ServiceClients** (v Infrastructure) - gRPC klienti

### 3. DDD Rules

- Entity v `*.Domain.Entities` musí dědit z `Entity` nebo `AggregateRoot`
- Aggregate roots musí dědit z `AggregateRoot`
- Domain events musí implementovat `IDomainEvent` a dědit z `DomainEventBase`
- Domain events musí být `sealed`

### 4. CQRS Rules

- Commands musí implementovat `ICommand<T>` nebo `ICommand`
- Queries musí implementovat `IQuery<T>`
- Handlers musí implementovat `IRequestHandler<,>` nebo `INotificationHandler<>`
- Validators musí dědit z `AbstractValidator<T>`
- Commands/Queries v Commands/Queries namespace

### 5. Structural Rules

- Controllers musí být pouze v API vrstvě
- DbContext musí být pouze v Infrastructure vrstvě
- Controllers musí končit na "Controller"

### 6. Naming Conventions

- Entity třídy končí na "Entity" (kromě owned entities jako OrderItem)
- Commands končí na "Command"
- Queries končí na "Query"
- Handlers končí na "Handler"
- Validators končí na "Validator"

**Poznámka:** Enum naming (E* prefix) je již vynucováno Roslyn analyzerem - není potřeba testovat.

## Sealed Classes - Roslyn Analyzer

Pro vynucení `sealed` na třídách bez potomků je vhodnější Roslyn analyzer než architektonický test:

**Důvody:**
- Poskytuje warning/error přímo v IDE při psaní kódu
- Funguje v compile-time, ne jen v test-time
- Lepší developer experience (okamžitá zpětná vazba)

**Možnosti:**
1. **Mezera.Analyzers** - obsahuje `MA0053: Make class sealed`
2. **SonarAnalyzer** - pravidlo `S4035`
3. **Vlastní analyzer** - pokud potřebujeme specifické chování

**Doporučení:** Přidat jako samostatný task (03) pro implementaci sealed analyzer.

## Implementation Steps

### Step 1: Setup projektu
1. Přidat `NetArchTest.Rules` do `Directory.Packages.props`:
   ```xml
   <ItemGroup Label="Testing">
     <!-- existing packages -->
     <PackageVersion Include="NetArchTest.Rules" Version="1.3.2" />
   </ItemGroup>
   ```
2. Vytvořit `tests/EShop.ArchitectureTests/` adresář
3. Vytvořit `.csproj`:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
       <ImplicitUsings>enable</ImplicitUsings>
       <Nullable>enable</Nullable>
       <IsPackable>false</IsPackable>
       <IsTestProject>true</IsTestProject>
       <RootNamespace>EShop.ArchitectureTests</RootNamespace>
     </PropertyGroup>

     <ItemGroup>
       <PackageReference Include="Microsoft.NET.Test.Sdk" />
       <PackageReference Include="MSTest.TestAdapter" />
       <PackageReference Include="MSTest.TestFramework" />
       <PackageReference Include="NetArchTest.Rules" />
       <PackageReference Include="coverlet.collector" />
     </ItemGroup>

     <ItemGroup>
       <ProjectReference Include="..\..\src\Services\Order\Order.Domain\Order.Domain.csproj" />
       <ProjectReference Include="..\..\src\Services\Order\Order.Application\Order.Application.csproj" />
       <ProjectReference Include="..\..\src\Services\Order\Order.Infrastructure\Order.Infrastructure.csproj" />
       <ProjectReference Include="..\..\src\Services\Order\Order.API\Order.API.csproj" />
       <ProjectReference Include="..\..\src\Services\Products\Products.Domain\Products.Domain.csproj" />
       <ProjectReference Include="..\..\src\Services\Products\Products.Application\Products.Application.csproj" />
       <ProjectReference Include="..\..\src\Services\Products\Products.Infrastructure\Products.Infrastructure.csproj" />
       <ProjectReference Include="..\..\src\Services\Products\Products.API\Products.API.csproj" />
       <ProjectReference Include="..\..\src\Common\EShop.SharedKernel\EShop.SharedKernel.csproj" />
       <ProjectReference Include="..\..\src\Common\EShop.Common\EShop.Common.csproj" />
       <ProjectReference Include="..\..\src\Common\EShop.Contracts\EShop.Contracts.csproj" />
     </ItemGroup>
   </Project>
   ```
4. Přidat projekt do `EShopDemo.sln`

### Step 2: Base infrastructure

**GlobalUsings.cs:**
```csharp
global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using NetArchTest.Rules;
global using EShop.ArchitectureTests.TestBase;
```

**TestBase/ArchitectureTestBase.cs:**
```csharp
using System.Reflection;

namespace EShop.ArchitectureTests.TestBase;

public abstract class ArchitectureTestBase
{
    // Order Service assemblies
    protected static readonly Assembly OrderDomainAssembly =
        typeof(Order.Domain.Entities.OrderEntity).Assembly;
    protected static readonly Assembly OrderApplicationAssembly =
        typeof(Order.Application.Commands.CreateOrder.CreateOrderCommand).Assembly;
    protected static readonly Assembly OrderInfrastructureAssembly =
        typeof(Order.Infrastructure.Data.OrderDbContext).Assembly;
    protected static readonly Assembly OrderApiAssembly =
        typeof(Order.API.Controllers.OrdersController).Assembly;

    // Products Service assemblies
    protected static readonly Assembly ProductsDomainAssembly =
        typeof(Products.Domain.Entities.ProductEntity).Assembly;
    protected static readonly Assembly ProductsApplicationAssembly =
        typeof(Products.Application.Commands.CreateProduct.CreateProductCommand).Assembly;
    protected static readonly Assembly ProductsInfrastructureAssembly =
        typeof(Products.Infrastructure.Data.ProductDbContext).Assembly;
    protected static readonly Assembly ProductsApiAssembly =
        typeof(Products.API.Controllers.ProductsController).Assembly;

    // Shared libraries
    protected static readonly Assembly SharedKernelAssembly =
        typeof(EShop.SharedKernel.Domain.Entity).Assembly;
    protected static readonly Assembly CommonAssembly =
        typeof(EShop.Common.Cqrs.ICommand).Assembly;

    protected static string FormatFailingTypes(TestResult result)
    {
        if (result.FailingTypes == null || !result.FailingTypes.Any())
            return "None";
        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
```

**TestBase/ServiceLayerDefinitions.cs:**
```csharp
namespace EShop.ArchitectureTests.TestBase;

public static class ServiceLayerDefinitions
{
    public static class Order
    {
        public const string Domain = "Order.Domain";
        public const string Application = "Order.Application";
        public const string Infrastructure = "Order.Infrastructure";
        public const string Api = "Order.API";
    }

    public static class Products
    {
        public const string Domain = "Products.Domain";
        public const string Application = "Products.Application";
        public const string Infrastructure = "Products.Infrastructure";
        public const string Api = "Products.API";
    }

    public static class Shared
    {
        public const string SharedKernel = "EShop.SharedKernel";
        public const string Common = "EShop.Common";
        public const string Contracts = "EShop.Contracts";
        public const string ServiceClients = "EShop.ServiceClients";
    }
}
```

### Step 3: Clean Architecture testy

**CleanArchitecture/DependencyTests.cs** - testuje:
- Domain nesmí referencovat Application, Infrastructure, API
- Domain smí referencovat pouze SharedKernel
- Application nesmí referencovat Infrastructure, API, **ServiceClients**
- Cross-service isolation (Order nesmí referencovat Products domain/application)

**CleanArchitecture/LayerIsolationTests.cs** - testuje:
- Controllers pouze v API vrstvě
- DbContext pouze v Infrastructure vrstvě

### Step 4: DDD testy

**DDD/EntityTests.cs** - testuje:
- Entity v Entities namespace dědí z Entity/AggregateRoot
- Aggregate roots dědí z AggregateRoot

**DDD/DomainEventTests.cs** - testuje:
- Domain events implementují IDomainEvent
- Domain events dědí z DomainEventBase
- Domain events jsou sealed
- Domain events jsou v Domain vrstvě (ne Application)

### Step 5: CQRS testy

**CQRS/CommandTests.cs** - testuje:
- Commands implementují ICommand<T> nebo ICommand
- Commands jsou v Commands namespace

**CQRS/QueryTests.cs** - testuje:
- Queries implementují IQuery<T>
- Queries jsou v Queries namespace

**CQRS/HandlerTests.cs** - testuje:
- Handlers implementují IRequestHandler nebo INotificationHandler
- CommandHandlers jsou v Commands namespace
- QueryHandlers jsou v Queries namespace

**CQRS/ValidatorTests.cs** - testuje:
- Validators dědí z AbstractValidator<T>
- CommandValidators jsou v Commands namespace

### Step 6: Naming conventions testy

**NamingConventions/EntityNamingTests.cs** - testuje:
- Entity třídy končí na "Entity" (s výjimkou owned entities)

**NamingConventions/CqrsNamingTests.cs** - testuje:
- Commands končí na "Command"
- Queries končí na "Query"
- Handlers končí na "Handler"

## Files Summary

| Action | File |
|--------|------|
| MODIFY | `Directory.Packages.props` - přidat NetArchTest.Rules |
| MODIFY | `EShopDemo.sln` - přidat testovací projekt |
| CREATE | `tests/EShop.ArchitectureTests/EShop.ArchitectureTests.csproj` |
| CREATE | `tests/EShop.ArchitectureTests/GlobalUsings.cs` |
| CREATE | `tests/EShop.ArchitectureTests/TestBase/ArchitectureTestBase.cs` |
| CREATE | `tests/EShop.ArchitectureTests/TestBase/ServiceLayerDefinitions.cs` |
| CREATE | `tests/EShop.ArchitectureTests/CleanArchitecture/DependencyTests.cs` |
| CREATE | `tests/EShop.ArchitectureTests/CleanArchitecture/LayerIsolationTests.cs` |
| CREATE | `tests/EShop.ArchitectureTests/DDD/EntityTests.cs` |
| CREATE | `tests/EShop.ArchitectureTests/DDD/DomainEventTests.cs` |
| CREATE | `tests/EShop.ArchitectureTests/CQRS/CommandTests.cs` |
| CREATE | `tests/EShop.ArchitectureTests/CQRS/QueryTests.cs` |
| CREATE | `tests/EShop.ArchitectureTests/CQRS/HandlerTests.cs` |
| CREATE | `tests/EShop.ArchitectureTests/CQRS/ValidatorTests.cs` |
| CREATE | `tests/EShop.ArchitectureTests/NamingConventions/EntityNamingTests.cs` |
| CREATE | `tests/EShop.ArchitectureTests/NamingConventions/CqrsNamingTests.cs` |

## Verification

```bash
# Build
dotnet build tests/EShop.ArchitectureTests

# Run all architecture tests
dotnet test tests/EShop.ArchitectureTests --verbosity normal

# Run specific test category
dotnet test tests/EShop.ArchitectureTests --filter "FullyQualifiedName~CleanArchitecture"
dotnet test tests/EShop.ArchitectureTests --filter "FullyQualifiedName~DDD"
dotnet test tests/EShop.ArchitectureTests --filter "FullyQualifiedName~CQRS"
```

## Notes

- **OrderItem** nedědí z `Entity` (je to owned entity) - testy mají výjimku
- Testy jsou parametrizované pro obě služby (Order, Products) pomocí `[DataRow]`
- Helper metoda `FormatFailingTypes()` pro čitelné chybové hlášky
- **Enum naming** (E* prefix) je řešeno Roslyn analyzerem - není v arch testech
- **Sealed classes** - doporučeno řešit Roslyn analyzerem (samostatný task)

## Related Tasks

- Task 03 (budoucí): Sealed Classes Roslyn Analyzer
