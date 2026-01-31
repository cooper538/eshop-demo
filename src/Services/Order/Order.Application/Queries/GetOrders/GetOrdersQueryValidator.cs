using FluentValidation;

namespace EShop.Order.Application.Queries.GetOrders;

public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    private const int MaxPageSize = 100;

    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);

        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
