using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazOrbit.Tests.Integration.Tests.Components.Carousel;

[Trait("Component Interaction", "BOBCarousel")]
public class BOBCarouselInteractionTests
{
    private static RenderFragment ThreeSlides => b =>
    {
        b.OpenComponent<BOBCarouselItem>(0);
        b.AddAttribute(1, nameof(BOBCarouselItem.ChildContent),
            (RenderFragment)(b2 => b2.AddContent(0, "S1")));
        b.CloseComponent();
        b.OpenComponent<BOBCarouselItem>(2);
        b.AddAttribute(3, nameof(BOBCarouselItem.ChildContent),
            (RenderFragment)(b2 => b2.AddContent(0, "S2")));
        b.CloseComponent();
        b.OpenComponent<BOBCarouselItem>(4);
        b.AddAttribute(5, nameof(BOBCarouselItem.ChildContent),
            (RenderFragment)(b2 => b2.AddContent(0, "S3")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Advance_On_Next_Arrow_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int? captured = null;
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides)
            .Add(c => c.ActiveIndexChanged, i => captured = i));

        // Act
        cut.Find(".bob-carousel__arrow--next").Click();

        // Assert
        captured.Should().Be(1);
        cut.Instance.ActiveIndex.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Go_Back_On_Previous_Arrow_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ActiveIndex, 2)
            .Add(c => c.ChildContent, ThreeSlides));

        // Act
        cut.Find(".bob-carousel__arrow--prev").Click();

        // Assert
        cut.Instance.ActiveIndex.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Prev_Arrow_At_Start_Without_Loop(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut.Find(".bob-carousel__arrow--prev").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Wrap_To_First_From_Last_When_Loop_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.Loop, true)
            .Add(c => c.ActiveIndex, 2)
            .Add(c => c.ChildContent, ThreeSlides));

        // Act
        cut.Find(".bob-carousel__arrow--next").Click();

        // Assert
        cut.Instance.ActiveIndex.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Wrap_To_Last_From_First_When_Loop_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.Loop, true)
            .Add(c => c.ActiveIndex, 0)
            .Add(c => c.ChildContent, ThreeSlides));

        // Act
        cut.Find(".bob-carousel__arrow--prev").Click();

        // Assert
        cut.Instance.ActiveIndex.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Jump_To_Slide_On_Indicator_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Act
        cut.FindAll(".bob-carousel__indicator")[2].Click();

        // Assert
        cut.Instance.ActiveIndex.Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_With_Arrow_Keys(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Act
        cut.Find("bob-component[data-bob-component='carousel']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        cut.Instance.ActiveIndex.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Jump_With_Home_End_Keys(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Act
        cut.Find("bob-component[data-bob-component='carousel']").KeyDown(new KeyboardEventArgs { Key = "End" });

        // Assert
        cut.Instance.ActiveIndex.Should().Be(2);

        cut.Find("bob-component[data-bob-component='carousel']").KeyDown(new KeyboardEventArgs { Key = "Home" });
        cut.Instance.ActiveIndex.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_Imperative_Navigation_Methods(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Act + Assert
        await cut.InvokeAsync(() => cut.Instance.GoToAsync(2));
        cut.Instance.ActiveIndex.Should().Be(2);

        await cut.InvokeAsync(() => cut.Instance.GoPreviousAsync());
        cut.Instance.ActiveIndex.Should().Be(1);

        await cut.InvokeAsync(() => cut.Instance.GoNextAsync());
        cut.Instance.ActiveIndex.Should().Be(2);
    }
}
