using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Carousel;

[Trait("Component Accessibility", "BOBCarousel")]
public class BOBCarouselAccessibilityTests
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
    public async Task Should_Root_Have_Carousel_Roledescription(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        var root = cut.Find("bob-component[data-bob-component='carousel']");
        root.GetAttribute("role").Should().Be("region");
        root.GetAttribute("aria-roledescription").Should().Be("carousel");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Slides_Have_Group_Role_And_Slide_Number_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        var slides = cut.FindAll(".bob-carousel__slide");
        slides[0].GetAttribute("role").Should().Be("group");
        slides[0].GetAttribute("aria-roledescription").Should().Be("slide");
        slides[0].GetAttribute("aria-label").Should().Be("Slide 1 of 3");
        slides[2].GetAttribute("aria-label").Should().Be("Slide 3 of 3");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Honor_Custom_Item_AriaLabel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        RenderFragment frag = b =>
        {
            b.OpenComponent<BOBCarouselItem>(0);
            b.AddAttribute(1, "AriaLabel", "Custom slide A");
            b.AddAttribute(2, nameof(BOBCarouselItem.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "A")));
            b.CloseComponent();
        };

        // Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, frag));

        // Assert
        cut.Find(".bob-carousel__slide").GetAttribute("aria-label").Should().Be("Custom slide A");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Inactive_Slides_Be_Aria_Hidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        var slides = cut.FindAll(".bob-carousel__slide");
        slides[0].GetAttribute("aria-hidden").Should().Be("false");
        slides[1].GetAttribute("aria-hidden").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Switch_Aria_Live_With_AutoPlay(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — non-autoplay → polite
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));
        cut.Find(".bob-carousel__viewport").GetAttribute("aria-live").Should().Be("polite");

        // Act — render autoplay variant
        IRenderedComponent<BOBCarousel> cut2 = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.AutoPlay, true)
            .Add(c => c.AutoPlayInterval, TimeSpan.FromHours(1))
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut2.Find(".bob-carousel__viewport").GetAttribute("aria-live").Should().Be("off");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Indicators_Have_Tab_Role_And_Aria_Current(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        var indicators = cut.FindAll(".bob-carousel__indicator");
        indicators[0].GetAttribute("role").Should().Be("tab");
        indicators[0].GetAttribute("aria-current").Should().Be("true");
        indicators[1].GetAttribute("aria-current").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Arrows_Have_Aria_Labels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BOBCarousel> cut = ctx.Render<BOBCarousel>(p => p
            .Add(c => c.PreviousLabel, "Anterior")
            .Add(c => c.NextLabel, "Siguiente")
            .Add(c => c.ChildContent, ThreeSlides));

        // Assert
        cut.Find(".bob-carousel__arrow--prev").GetAttribute("aria-label").Should().Be("Anterior");
        cut.Find(".bob-carousel__arrow--next").GetAttribute("aria-label").Should().Be("Siguiente");
    }
}
