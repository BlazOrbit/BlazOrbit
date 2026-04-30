using BlazOrbit.Abstractions;
using BlazOrbit.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using NSubstitute;

namespace BlazOrbit.Tests.Integration.Tests.Library;

/// <summary>
/// Direct unit tests for <see cref="VariantHelper{TComponent, TVariant}" />.
/// Pins the resolve order (built-in templates first, then <see cref="IVariantRegistry" />,
/// null-safety for both branches).
/// </summary>
[Trait("Core", "VariantHelper")]
public class VariantHelperTests
{
    [Fact]
    public void ResolveTemplate_Should_Return_BuiltIn_When_Variant_Is_Registered_In_BuiltIn_Dictionary()
    {
        TestComponent component = new();
        IVariantRegistry registry = Substitute.For<IVariantRegistry>();
        TestVariant filled = new("Filled");
        RenderFragment builtInFragment = _ => { };
        Dictionary<TestVariant, Func<TestComponent, RenderFragment>> builtIns = new()
        {
            [filled] = _ => builtInFragment
        };

        VariantHelper<TestComponent, TestVariant> helper = new(component, registry);

        RenderFragment? resolved = helper.ResolveTemplate(filled, builtIns);

        resolved.Should().BeSameAs(builtInFragment);
        registry.DidNotReceive().GetTemplate(
            Arg.Any<Type>(), Arg.Any<Variant>(), Arg.Any<ComponentBase>());
    }

    [Fact]
    public void ResolveTemplate_Should_Fall_Through_To_Registry_When_BuiltIn_Does_Not_Contain_Variant()
    {
        TestComponent component = new();
        IVariantRegistry registry = Substitute.For<IVariantRegistry>();
        TestVariant filled = new("Filled");
        TestVariant outlined = new("Outlined");
        RenderFragment registryFragment = _ => { };
        Dictionary<TestVariant, Func<TestComponent, RenderFragment>> builtIns = new()
        {
            [filled] = _ => _ => { }
        };
        registry
            .GetTemplate(typeof(TestComponent), outlined, component)
            .Returns(registryFragment);

        VariantHelper<TestComponent, TestVariant> helper = new(component, registry);

        RenderFragment? resolved = helper.ResolveTemplate(outlined, builtIns);

        resolved.Should().BeSameAs(registryFragment);
    }

    [Fact]
    public void ResolveTemplate_Should_Return_Null_When_No_BuiltIn_And_Registry_Returns_Null()
    {
        TestComponent component = new();
        IVariantRegistry registry = Substitute.For<IVariantRegistry>();
        TestVariant outlined = new("Outlined");
        registry
            .GetTemplate(Arg.Any<Type>(), Arg.Any<Variant>(), Arg.Any<ComponentBase>())
            .Returns((RenderFragment?)null);

        VariantHelper<TestComponent, TestVariant> helper = new(component, registry);

        RenderFragment? resolved = helper.ResolveTemplate(outlined, builtInTemplates: null);

        resolved.Should().BeNull();
    }

    [Fact]
    public void ResolveTemplate_Should_Return_Null_When_BuiltIn_Null_And_Registry_Null()
    {
        TestComponent component = new();
        TestVariant variant = new("Outlined");

        VariantHelper<TestComponent, TestVariant> helper = new(component, registry: null);

        RenderFragment? resolved = helper.ResolveTemplate(variant, builtInTemplates: null);

        resolved.Should().BeNull();
    }

    [Fact]
    public void ResolveTemplate_Should_Use_Registry_When_BuiltIn_Dictionary_Is_Null()
    {
        TestComponent component = new();
        IVariantRegistry registry = Substitute.For<IVariantRegistry>();
        TestVariant variant = new("Outlined");
        RenderFragment registryFragment = _ => { };
        registry
            .GetTemplate(typeof(TestComponent), variant, component)
            .Returns(registryFragment);

        VariantHelper<TestComponent, TestVariant> helper = new(component, registry);

        RenderFragment? resolved = helper.ResolveTemplate(variant, builtInTemplates: null);

        resolved.Should().BeSameAs(registryFragment);
    }

    [Fact]
    public void ResolveTemplate_Should_Pass_Component_Instance_To_BuiltIn_Factory()
    {
        TestComponent component = new();
        TestVariant variant = new("Filled");
        TestComponent? received = null;
        Dictionary<TestVariant, Func<TestComponent, RenderFragment>> builtIns = new()
        {
            [variant] = c =>
            {
                received = c;
                return _ => { };
            }
        };

        VariantHelper<TestComponent, TestVariant> helper = new(component, registry: null);

        helper.ResolveTemplate(variant, builtIns);

        received.Should().BeSameAs(component);
    }

    [Fact]
    public void ResolveTemplate_Should_Pass_Runtime_Component_Type_To_Registry_Not_Generic_Base()
    {
        DerivedTestComponent component = new();
        IVariantRegistry registry = Substitute.For<IVariantRegistry>();
        TestVariant variant = new("Outlined");

        VariantHelper<TestComponent, TestVariant> helper = new(component, registry);

        helper.ResolveTemplate(variant, builtInTemplates: null);

        registry.Received(1).GetTemplate(typeof(DerivedTestComponent), variant, component);
    }

    [Fact]
    public void ResolveTemplate_Should_Prefer_BuiltIn_Over_Registry_When_Both_Match()
    {
        TestComponent component = new();
        IVariantRegistry registry = Substitute.For<IVariantRegistry>();
        TestVariant variant = new("Filled");
        RenderFragment builtInFragment = _ => { };
        RenderFragment registryFragment = _ => { };
        Dictionary<TestVariant, Func<TestComponent, RenderFragment>> builtIns = new()
        {
            [variant] = _ => builtInFragment
        };
        registry
            .GetTemplate(Arg.Any<Type>(), Arg.Any<Variant>(), Arg.Any<ComponentBase>())
            .Returns(registryFragment);

        VariantHelper<TestComponent, TestVariant> helper = new(component, registry);

        RenderFragment? resolved = helper.ResolveTemplate(variant, builtIns);

        resolved.Should().BeSameAs(builtInFragment);
        resolved.Should().NotBeSameAs(registryFragment);
    }

    // ─────────── Stubs ───────────

    private class TestComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder) { }
    }

    private sealed class DerivedTestComponent : TestComponent;

    private sealed class TestVariant : Variant
    {
        public TestVariant(string name) : base(name) { }
    }
}
