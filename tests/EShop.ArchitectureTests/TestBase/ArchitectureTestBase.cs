using System.Reflection;

namespace EShop.ArchitectureTests.TestBase;

public static class AssemblyExtensions
{
    public static string Name(this Assembly assembly) => assembly.GetName().Name!;
}

public abstract class ArchitectureTestBase
{
    // Order Service assemblies
    protected static readonly Assembly OrderDomainAssembly =
        typeof(EShop.Order.Domain.Entities.OrderEntity).Assembly;

    protected static readonly Assembly OrderApplicationAssembly =
        typeof(EShop.Order.Application.Commands.CreateOrder.CreateOrderCommand).Assembly;

    protected static readonly Assembly OrderInfrastructureAssembly =
        typeof(EShop.Order.Infrastructure.Data.OrderDbContext).Assembly;

    protected static readonly Assembly OrderApiAssembly =
        typeof(EShop.Order.API.Controllers.OrdersController).Assembly;

    // Products Service assemblies
    protected static readonly Assembly ProductsDomainAssembly =
        typeof(EShop.Products.Domain.Entities.ProductEntity).Assembly;

    protected static readonly Assembly ProductsApplicationAssembly =
        typeof(EShop.Products.Application.Commands.CreateProduct.CreateProductCommand).Assembly;

    protected static readonly Assembly ProductsInfrastructureAssembly =
        typeof(EShop.Products.Infrastructure.Data.ProductDbContext).Assembly;

    protected static readonly Assembly ProductsApiAssembly =
        typeof(EShop.Products.API.Controllers.ProductsController).Assembly;

    // Shared libraries
    protected static readonly Assembly SharedKernelAssembly =
        typeof(EShop.SharedKernel.Domain.Entity).Assembly;

    protected static readonly Assembly CommonAssembly =
        typeof(EShop.Common.Application.Cqrs.ICommand).Assembly;

    protected static readonly Assembly ContractsAssembly =
        typeof(EShop.Contracts.ServiceClients.Product.IProductServiceClient).Assembly;

    protected static readonly Assembly ServiceClientsAssembly =
        typeof(EShop.ServiceClients.Clients.Product.GrpcProductServiceClient).Assembly;

    // Infrastructure services (no Clean Architecture layers)
    protected static readonly Assembly GatewayApiAssembly =
        typeof(EShop.Gateway.API.Configuration.GatewaySettings).Assembly;

    protected static readonly Assembly NotificationAssembly =
        typeof(EShop.NotificationService.Consumers.OrderConfirmedConsumer).Assembly;

    // Assembly lookup dictionary for DataRow tests
    private static readonly Dictionary<string, Assembly> Assemblies = new()
    {
        [nameof(OrderDomainAssembly)] = OrderDomainAssembly,
        [nameof(OrderApplicationAssembly)] = OrderApplicationAssembly,
        [nameof(OrderInfrastructureAssembly)] = OrderInfrastructureAssembly,
        [nameof(OrderApiAssembly)] = OrderApiAssembly,
        [nameof(ProductsDomainAssembly)] = ProductsDomainAssembly,
        [nameof(ProductsApplicationAssembly)] = ProductsApplicationAssembly,
        [nameof(ProductsInfrastructureAssembly)] = ProductsInfrastructureAssembly,
        [nameof(ProductsApiAssembly)] = ProductsApiAssembly,
        [nameof(SharedKernelAssembly)] = SharedKernelAssembly,
        [nameof(CommonAssembly)] = CommonAssembly,
        [nameof(ContractsAssembly)] = ContractsAssembly,
        [nameof(ServiceClientsAssembly)] = ServiceClientsAssembly,
        [nameof(GatewayApiAssembly)] = GatewayApiAssembly,
        [nameof(NotificationAssembly)] = NotificationAssembly,
    };

    #region Assembly Helpers

    /// <summary>
    /// Get assembly by field name (for DataRow parameterized tests).
    /// </summary>
    protected static Assembly GetAssembly(string name) =>
        Assemblies.TryGetValue(name, out var assembly)
            ? assembly
            : throw new ArgumentException($"Unknown assembly: {name}");

    #endregion

    #region Assertion Helpers

    /// <summary>
    /// Assert that NetArchTest result has no violations.
    /// </summary>
    protected static void AssertNoViolations(NetArchTest.Rules.TestResult result, string rule)
    {
        Assert.IsTrue(result.IsSuccessful, $"{rule}\nViolating types:\n{FormatTypes(result)}");
    }

    /// <summary>
    /// Assert that type list is empty (no violations).
    /// </summary>
    protected static void AssertNoViolatingTypes(IReadOnlyList<Type> types, string rule)
    {
        Assert.AreEqual(0, types.Count, $"{rule}\nViolating types:\n{FormatTypes(types)}");
    }

    /// <summary>
    /// Assert that all types match the predicate.
    /// </summary>
    protected static void AssertAllTypes(
        IEnumerable<Type> types,
        Func<Type, bool> predicate,
        Func<Type, string> errorMessage
    )
    {
        foreach (var type in types)
        {
            Assert.IsTrue(predicate(type), errorMessage(type));
        }
    }

    #endregion

    #region Reflection Helpers

    /// <summary>
    /// Check if type implements a generic interface.
    /// </summary>
    protected static bool ImplementsGenericInterface(Type type, Type genericInterface)
    {
        return type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface);
    }

    /// <summary>
    /// Check if type implements any of the specified generic interfaces.
    /// </summary>
    protected static bool ImplementsAnyGenericInterface(Type type, params Type[] genericInterfaces)
    {
        return type.GetInterfaces()
            .Any(i => i.IsGenericType && genericInterfaces.Contains(i.GetGenericTypeDefinition()));
    }

    /// <summary>
    /// Check if type implements interface (generic or non-generic).
    /// </summary>
    protected static bool ImplementsInterface(Type type, Type iface)
    {
        return type.GetInterfaces()
            .Any(i => i == iface || (i.IsGenericType && i.GetGenericTypeDefinition() == iface));
    }

    /// <summary>
    /// Check if type is a C# record.
    /// </summary>
    protected static bool IsRecord(Type type)
    {
        // Records have a compiler-generated method called <Clone>$
        var cloneMethod = type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance);
        if (cloneMethod != null)
        {
            return true;
        }

        // Alternative check: records have EqualityContract property
        var equalityContract = type.GetProperty(
            "EqualityContract",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        return equalityContract != null;
    }

    /// <summary>
    /// Check if type inherits from a generic base class.
    /// </summary>
    protected static bool IsSubclassOfGeneric(Type type, Type genericBase)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericBase)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    #endregion

    #region Formatting Helpers

    /// <summary>
    /// Format NetArchTest result failing types.
    /// </summary>
    protected static string FormatTypes(NetArchTest.Rules.TestResult result)
    {
        if (result.FailingTypes == null || !result.FailingTypes.Any())
        {
            return "None";
        }

        return FormatTypes(result.FailingTypes);
    }

    /// <summary>
    /// Format type list for error messages.
    /// </summary>
    protected static string FormatTypes(IEnumerable<Type> types)
    {
        var list = types.ToList();
        return list.Count == 0
            ? "None"
            : string.Join(Environment.NewLine, list.Select(t => $"  - {t.FullName}"));
    }

    // Keep old method for backward compatibility during migration
    protected static string FormatFailingTypes(NetArchTest.Rules.TestResult result) =>
        FormatTypes(result);

    #endregion
}
