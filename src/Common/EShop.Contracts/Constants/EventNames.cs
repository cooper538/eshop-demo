namespace EShop.Contracts.Constants;

public static class EventNames
{
    public static class Orders
    {
        public const string Confirmed = "order.confirmed";
        public const string Rejected = "order.rejected";
        public const string Cancelled = "order.cancelled";
    }

    public static class Products
    {
        public const string Created = "product.created";
        public const string Updated = "product.updated";
        public const string StockLow = "product.stock-low";
    }
}
