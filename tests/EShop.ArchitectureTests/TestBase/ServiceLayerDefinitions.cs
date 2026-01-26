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
