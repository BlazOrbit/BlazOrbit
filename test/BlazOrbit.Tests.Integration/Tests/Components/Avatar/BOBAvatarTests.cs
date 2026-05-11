using BlazOrbit.Components;
using BlazOrbit.Components.Display;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Avatar;

[Trait("Component Rendering", "BOBAvatar")]
public class BOBAvatarRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Initials_When_No_Image(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatar> cut = ctx.Render<BOBAvatar>(p => p
            .Add(c => c.Label, "Ada Lovelace"));

        cut.Find("bob-component").GetAttribute("data-bob-component").Should().Be("avatar");
        cut.Find(".bob-avatar__initials").TextContent.Should().Be("AL");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Single_Initial_For_One_Word_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatar> cut = ctx.Render<BOBAvatar>(p => p
            .Add(c => c.Label, "ada"));

        cut.Find(".bob-avatar__initials").TextContent.Should().Be("A");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Question_Mark_For_Empty_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatar> cut = ctx.Render<BOBAvatar>();

        cut.Find(".bob-avatar__initials").TextContent.Should().Be("?");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Image_When_Url_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatar> cut = ctx.Render<BOBAvatar>(p => p
            .Add(c => c.ImageUrl, "/me.png")
            .Add(c => c.Label, "Me"));

        cut.Find(".bob-avatar__img").GetAttribute("src").Should().Be("/me.png");
        cut.FindAll(".bob-avatar__initials").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Shape_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatar> cut = ctx.Render<BOBAvatar>(p => p
            .Add(c => c.Label, "x")
            .Add(c => c.Shape, BOBAvatarShape.Rounded));

        cut.Find(".bob-avatar").GetAttribute("data-bob-shape").Should().Be("rounded");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Status_Dot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatar> cut = ctx.Render<BOBAvatar>(p => p
            .Add(c => c.Label, "x")
            .Add(c => c.Status, BOBAvatarStatus.Online));

        cut.Find(".bob-avatar__status").GetAttribute("data-bob-status").Should().Be("online");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Generate_Deterministic_Gradient(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatar> cut1 = ctx.Render<BOBAvatar>(p => p.Add(c => c.Label, "Ada"));
        IRenderedComponent<BOBAvatar> cut2 = ctx.Render<BOBAvatar>(p => p.Add(c => c.Label, "Ada"));

        cut1.Find(".bob-avatar").GetAttribute("style").Should().Be(cut2.Find(".bob-avatar").GetAttribute("style"));
    }
}

[Trait("Component Rendering", "BOBAvatarGroup")]
public class BOBAvatarGroupRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .AddChildContent<BOBAvatar>(c => c.Add(a => a.Label, "A"))
            .AddChildContent<BOBAvatar>(c => c.Add(a => a.Label, "B"))
            .AddChildContent<BOBAvatar>(c => c.Add(a => a.Label, "C")));

        cut.FindAll(".bob-avatar").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Overflow_When_TotalOverride_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.MaxVisible, 3)
            .Add(c => c.TotalOverride, 8)
            .AddChildContent<BOBAvatar>(c => c.Add(a => a.Label, "A")));

        cut.Find(".bob-avatar-group__overflow").TextContent.Should().Be("+5");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Skip_Overflow_When_Total_Below_MaxVisible(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBAvatarGroup> cut = ctx.Render<BOBAvatarGroup>(p => p
            .Add(c => c.MaxVisible, 5)
            .Add(c => c.TotalOverride, 3));

        cut.FindAll(".bob-avatar-group__overflow").Should().BeEmpty();
    }
}

[Trait("Component Snapshots", "BOBAvatar")]
public class BOBAvatarSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var avatars = new[]
        {
            new { Name = "Initials", Builder = (Action<ComponentParameterCollectionBuilder<BOBAvatar>>)(p => p
                .Add(c => c.Label, "Ada Lovelace")) },
            new { Name = "Image", Builder = (Action<ComponentParameterCollectionBuilder<BOBAvatar>>)(p => p
                .Add(c => c.ImageUrl, "/me.png")
                .Add(c => c.Label, "Me")) },
            new { Name = "Square_Online", Builder = (Action<ComponentParameterCollectionBuilder<BOBAvatar>>)(p => p
                .Add(c => c.Label, "John Doe")
                .Add(c => c.Shape, BOBAvatarShape.Square)
                .Add(c => c.Status, BOBAvatarStatus.Online)) }
        };

        var results = avatars.Select(tc =>
        {
            IRenderedComponent<BOBAvatar> cut = ctx.Render<BOBAvatar>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
