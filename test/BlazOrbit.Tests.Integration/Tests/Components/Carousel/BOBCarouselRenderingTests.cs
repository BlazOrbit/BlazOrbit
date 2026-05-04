using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Carousel;

[Trait("Component Rendering", "BOBCarousel")]
public class BOBCarouselRenderingTests
{
    private static RenderFragment ThreeSlides => b =>
    {
        b.OpenComponent<BOBCarouselItem>(0);
        b.AddAttribute(1, nameof(BOBCarouselItem.ChildContent),
            (RenderFragment)(b2 => b2.AddContent(0, "Slide 1")));
        b.CloseComponent();
        b.OpenComponent<BOBCarouselItem>(2);
        b.AddAttribute(3, nameof(BOBCarouselItem.ChildContent),
            (RenderFragment)(b2 => b2.AddContent(0, "Slide 2")));
        b.CloseComponent();
        b.OpenComponent<BOBCarouselItem>(4);
        b.AddAttribute(5, nameof(BOBCarouselItem.ChildContent),
            (RenderFragment)(b2 => b2.AddContent(0, "Slide 3")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut.Find("bob-component[data-bob-component='carousel']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_All_Slides(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut.FindAll(".bob-carousel__slide").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_First_Slide_Active_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        var slides = cut.FindAll(".bob-carousel__slide");
        slides[0].GetAttribute("data-bob-active").Should().Be("true");
        slides[1].GetAttribute("data-bob-active").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Arrows_When_Multiple_Slides(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut.Find(".bob-carousel__arrow--prev").Should().NotBeNull();
        cut.Find(".bob-carousel__arrow--next").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Arrows_When_ShowArrows_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ShowArrows, false)
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut.FindAll(".bob-carousel__arrow").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_One_Indicator_Per_Slide(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut.FindAll(".bob-carousel__indicator").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dot_When_IndicatorIcon_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut.FindAll(".bob-carousel__indicator-dot").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_SvgIcon_When_IndicatorIcon_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.IndicatorIcon, BOBIconKeys.UI.Plus)
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert — no dots, three svg icons inside indicators
        cut.FindAll(".bob-carousel__indicator-dot").Should().BeEmpty();
        cut.FindAll(".bob-carousel__indicator bob-component[data-bob-component='svg-icon']").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Transition_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.Transition, BOBCarouselTransition.Fade)
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut.Find(".bob-carousel__viewport").GetAttribute("data-bob-anim").Should().Be("fade");
        cut.Find("bob-component[data-bob-component='carousel']").GetAttribute("data-bob-anim").Should().Be("fade");
    }
}
