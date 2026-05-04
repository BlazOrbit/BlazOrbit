using BlazOrbit.Components;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazOrbit.Tests.Integration.Tests.Components.Carousel;

[Trait("Component Snapshots", "BOBCarousel")]
public class BOBCarouselSnapshotTests
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
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Slide_Default_FirstActive",
                Html = ctx.Render<BOBCarousel>(p => p
                    .Add(c => c.ChildContent, ThreeSlides)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Slide_SecondActive",
                Html = ctx.Render<BOBCarousel>(p => p
                    .Add(c => c.ActiveIndex, 1)
                    .Add(c => c.ChildContent, ThreeSlides)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Fade_Variant",
                Html = ctx.Render<BOBCarousel>(p => p
                    .Add(c => c.Transition, BOBCarouselTransition.Fade)
                    .Add(c => c.ChildContent, ThreeSlides)).GetNormalizedMarkup()
            },
            new
            {
                Name = "No_Indicators_No_Arrows",
                Html = ctx.Render<BOBCarousel>(p => p
                    .Add(c => c.ShowArrows, false)
                    .Add(c => c.ShowIndicators, false)
                    .Add(c => c.ChildContent, ThreeSlides)).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
