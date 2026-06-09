using AngleSharp.Dom;
using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Select;

[Trait("Component Rendering", "BOBSelect")]
public class BOBSelectRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Native_Select_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Value, "opt1")
            .Add(c => c.ChildContent, b =>
            {
                b.AddContent(0, "<option value='opt1'>One</option>");
            }));

        cut.Find("select.bob-select__native").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_When_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Label, "Country")
            .Add(c => c.Value, "es"));

        cut.Find("label.bob-select__label").TextContent.Should().Be("Country");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Helper_Text_When_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.HelperText, "Pick one")
            .Add(c => c.Value, "a"));

        cut.Find(".bob-field__helper").TextContent.Should().Be("Pick one");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Indicator_When_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Label, "Country")
            .Add(c => c.Required, true));

        cut.Find("span.bob-field__required").TextContent.Should().Be("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Child_Options(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBSelect<string>> cut = ctx.Render<BOBSelect<string>>(p => p
            .Add(c => c.Value, "b")
            .Add(c => c.ChildContent, b => b.AddMarkupContent(0,
                "<option value='a'>A</option><option value='b'>B</option>")));

        IReadOnlyList<IElement> options = cut.FindAll("select option");
        options.Should().HaveCount(2);
        options[0].GetAttribute("value").Should().Be("a");
        options[1].GetAttribute("value").Should().Be("b");
    }
}
