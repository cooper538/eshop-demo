using EShop.Order.Domain.Enums;

namespace EShop.Order.Domain.Exceptions;

public class InvalidOrderStateException : Exception
{
    public EOrderStatus CurrentStatus { get; }
    public EOrderStatus TargetStatus { get; }

    public InvalidOrderStateException(EOrderStatus currentStatus, EOrderStatus targetStatus)
        : base($"Cannot transition order from {currentStatus} to {targetStatus}")
    {
        CurrentStatus = currentStatus;
        TargetStatus = targetStatus;
    }
}
