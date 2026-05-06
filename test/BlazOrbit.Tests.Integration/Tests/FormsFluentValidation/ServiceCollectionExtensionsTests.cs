using BlazOrbit.FormsFluentValidation;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.FormsFluentValidation;

[Trait("FormsFluentValidation", "ServiceCollectionExtensions")]
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBOBFluentValidation_Generic_Should_Register_Validators_From_Containing_Assembly()
    {
        var services = new ServiceCollection();

        services.AddBOBFluentValidation<BOBFluentValidatorTests.TestModelValidator>();

        services.Should().Contain(d =>
            d.ServiceType == typeof(IValidator<BOBFluentValidatorTests.TestModel>));
    }

    [Fact]
    public void AddBOBFluentValidation_Assembly_Should_Register_Validators_From_Provided_Assembly()
    {
        var services = new ServiceCollection();

        services.AddBOBFluentValidation(typeof(BOBFluentValidatorTests.TestModelValidator).Assembly);

        services.Should().Contain(d =>
            d.ServiceType == typeof(IValidator<BOBFluentValidatorTests.TestModel>));
    }

    [Fact]
    public void AddBOBFluentValidation_Should_Return_Same_ServiceCollection()
    {
        var services = new ServiceCollection();

        IServiceCollection result = services.AddBOBFluentValidation<BOBFluentValidatorTests.TestModelValidator>();

        result.Should().BeSameAs(services);
    }
}
