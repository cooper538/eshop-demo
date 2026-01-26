using EShop.Common.Cqrs;
using FluentValidation;
using MediatR;

namespace EShop.ArchitectureTests.Conventions;

public partial class NamingConventionTests
{
    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void TypesImplementingICommand_ShouldEndWith_Command(string assemblyFieldName)
    {
        var commandTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .AreNotAbstract()
            .GetTypes()
            .Where(t =>
                ImplementsInterface(t, typeof(ICommand))
                || ImplementsInterface(t, typeof(ICommand<>))
            );

        var violatingTypes = commandTypes.Where(t => !t.Name.EndsWith("Command")).ToList();

        AssertNoViolatingTypes(
            violatingTypes,
            "All types implementing ICommand should end with 'Command'."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void TypesImplementingIQuery_ShouldEndWith_Query(string assemblyFieldName)
    {
        var queryTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => ImplementsGenericInterface(t, typeof(IQuery<>)));

        var violatingTypes = queryTypes.Where(t => !t.Name.EndsWith("Query")).ToList();

        AssertNoViolatingTypes(
            violatingTypes,
            "All types implementing IQuery should end with 'Query'."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void TypesImplementingIRequestHandler_ShouldEndWith_Handler(string assemblyFieldName)
    {
        var handlerTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t =>
                ImplementsAnyGenericInterface(
                    t,
                    typeof(IRequestHandler<>),
                    typeof(IRequestHandler<,>),
                    typeof(INotificationHandler<>)
                )
            );

        var violatingTypes = handlerTypes.Where(t => !t.Name.EndsWith("Handler")).ToList();

        AssertNoViolatingTypes(
            violatingTypes,
            "All types implementing IRequestHandler/INotificationHandler should end with 'Handler'."
        );
    }

    [TestMethod]
    [DataRow(nameof(OrderApplicationAssembly))]
    [DataRow(nameof(ProductsApplicationAssembly))]
    public void Validators_ShouldEndWith_Validator(string assemblyFieldName)
    {
        var validatorTypes = Types
            .InAssembly(GetAssembly(assemblyFieldName))
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => IsSubclassOfGeneric(t, typeof(AbstractValidator<>)));

        var violatingTypes = validatorTypes.Where(t => !t.Name.EndsWith("Validator")).ToList();

        AssertNoViolatingTypes(violatingTypes, "All validators should end with 'Validator'.");
    }
}
