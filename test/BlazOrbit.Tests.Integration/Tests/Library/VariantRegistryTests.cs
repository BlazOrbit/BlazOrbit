using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using BlazOrbit.Tests.Integration.Templates.Components;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Functional contract tests for <see cref="IVariantRegistry" />.
///
/// These tests define and enforce the following system-level rules:
///
/// 1 - Variants MUST NOT be registered directly through <see cref="IVariantRegistry" /> at runtime.
/// 2 - Variants MUST be registered during application startup via <c> AddBlazOrbitVariants </c>.
///
/// 3 - Variant registration is cumulative during startup and supports multiple calls to <c>
/// AddBlazOrbitVariants </c>.
///
/// 4 - When multiple registrations target the same component + variant key, the last registration
/// wins (deterministic override).
///
/// 5 - Registered variants can be resolved and executed correctly at render time.
///
/// 6 - Requests for non-registered variants MUST return <c> null </c> and never throw.
///
/// 7 - Calling <c> AddBlazOrbitVariants </c> without first calling <c> AddBlazOrbit </c> is
/// considered an invalid configuration and MUST fail when the registry is resolved.
///
/// 8 - The same <see cref="Variant" /> instance MAY be registered for multiple component types.
/// This is supported but not recommended; consumers are encouraged to define component-specific
/// variant classes for clarity and maintainability.
///
/// 9 - Variants registered for a base component type MUST be resolvable by derived components.
///
/// Thread-safety:
/// - Variant registration is NOT thread-safe and is restricted to application startup.
/// - Variant resolution is read-only after startup and is safe for concurrent use.
/// - Thread-safety guarantees are by design and documented here, not enforced by tests.
///
/// Together, these tests define the immutability, lifecycle, resolution and usage semantics of the
/// Variant Registry.
/// </summary>
[Trait("Library", "VariantRegistry")]
public class VariantRegistryTests
{
    private readonly TestVariantComponent_CustomVariants _templates = new();

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task VariantRegistry_Allows_Same_Variant_For_Multiple_Components(
        BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        TestVariant sharedVariant = TestVariant.Custom("Shared");

        ctx.Services.AddBlazOrbitVariants(builder =>
        {
            builder.ForComponent<TestVariantComponent>()
                   .AddVariant(sharedVariant, _templates.BasicCustomTemplate);

            builder.ForComponent<DerivedTestVariantComponent>()
                   .AddVariant(sharedVariant, _templates.BasicCustomTemplate);
        });

        IVariantRegistry registry = ctx.Services.GetRequiredService<IVariantRegistry>();

        registry.GetTemplate(
            typeof(TestVariantComponent),
            sharedVariant,
            null!).Should().NotBeNull();

        registry.GetTemplate(
            typeof(DerivedTestVariantComponent),
            sharedVariant,
            null!).Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task VariantRegistry_GetTemplate_ReturnsNullForUnregistered(
        BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        TestVariant variant = TestVariant.Custom("Unregistered");
        IVariantRegistry registry = ctx.Services.GetRequiredService<IVariantRegistry>();

        RenderFragment? template =
            registry.GetTemplate(
                typeof(TestVariantComponent),
                variant,
                null!);

        template.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task VariantRegistry_Register_OverwritesExisting(
        BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        TestVariant variant = TestVariant.Custom("Test");
        bool firstCalled = false;
        bool secondCalled = false;

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<TestVariantComponent>()
                   .AddVariant(
                       variant,
                       _ => __builder => firstCalled = true));

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<TestVariantComponent>()
                   .AddVariant(
                       variant,
                       _ => __builder => secondCalled = true));

        IVariantRegistry registry = ctx.Services.GetRequiredService<IVariantRegistry>();
        RenderFragment? retrieved =
            registry.GetTemplate(typeof(TestVariantComponent), variant, null!);

        retrieved?.Invoke(null!);

        firstCalled.Should().BeFalse();
        secondCalled.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task VariantRegistry_Register_StoresTemplate(
        BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        TestVariant variant = TestVariant.Custom("Test");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<TestVariantComponent>()
                   .AddVariant(
                       variant,
                       _ => __builder => { }));

        IVariantRegistry registry = ctx.Services.GetRequiredService<IVariantRegistry>();
        RenderFragment? retrieved =
            registry.GetTemplate(typeof(TestVariantComponent), variant, null!);

        retrieved.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task VariantRegistry_Register_Throws_WhenCalledAfterStartup(
        BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IVariantRegistry registry = ctx.Services.GetRequiredService<IVariantRegistry>();
        TestVariant variant = TestVariant.Custom("Late");

        Action act = () =>
            registry.Register<TestVariantComponent, TestVariant>(
                variant,
                _ => __builder => { });

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Variants must be registered during startup");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task VariantRegistry_Resolves_Variants_From_Base_Component(
        BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        TestVariant inheritedVariant = TestVariant.Custom("Inherited");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<TestVariantComponent>()
                   .AddVariant(inheritedVariant, _templates.BasicCustomTemplate));

        IVariantRegistry registry = ctx.Services.GetRequiredService<IVariantRegistry>();

        RenderFragment? template =
            registry.GetTemplate(
                typeof(DerivedTestVariantComponent),
                inheritedVariant,
                null!);

        template.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task VariantRegistry_Should_Register_And_Retrieve_Custom_Variants(
        BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        TestVariant customVariant = TestVariant.Custom("Custom");

        ctx.Services.AddBlazOrbitVariants(builder =>
            builder.ForComponent<TestVariantComponent>()
                   .AddVariant(customVariant, _templates.BasicCustomTemplate));

        IRenderedComponent<TestVariantComponent> cut =
            ctx.Render<TestVariantComponent>(parameters => parameters
                .Add(p => p.Variant, customVariant)
                .Add(p => p.Text, "Custom Component"));

        cut.Find("button").TextContent.Should().Be("Custom Component");
    }

    [Fact]
    public void VariantRegistry_Throws_When_AddBlazOrbit_IsMissing()
    {
        ServiceCollection services = new();

        services.AddBlazOrbitVariants(_ => { });

        Action act = () =>
            services.BuildServiceProvider()
                    .GetRequiredService<IVariantRegistry>();

        act.Should().Throw<InvalidOperationException>();
    }
}