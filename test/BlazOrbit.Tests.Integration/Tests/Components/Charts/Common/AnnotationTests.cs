using BlazOrbit.Charts.Components;
using BlazOrbit.Charts.Models;
using BlazOrbit.Tests.Integration.Infrastructure;
using BlazOrbit.Tests.Integration.Infrastructure.Contexts;
using Bunit;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Components.Charts.Common;

[Trait("Component Variant", "Charts.Annotations")]
public class AnnotationTests
{
    private static IEnumerable<BOBChartSeries<int, double>> Series10() => new[]
    {
        new BOBChartSeries<int, double>
        {
            Label = "S",
            Points = Enumerable.Range(1, 10)
                .Select(i => new BOBChartPoint<int, double>(i, i * 1.0))
                .ToArray()
        }
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Annotations_Default_Should_Be_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p.Add(c => c.Series, Series10()));

        cut.Instance.Annotations.Should().BeNull();
        cut.FindAll("g.bob-chart__annotations").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Annotations_Empty_Should_Render_Empty_Group(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, Array.Empty<BOBChartAnnotation>()));

        cut.FindAll("g.bob-chart__annotations").Should().HaveCount(1);
        cut.FindAll(".bob-chart__annotation-text").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Text_Annotation_Should_Render_With_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartTextAnnotation<int, double>
                    {
                        X = 5, Y = 5.0, Text = "Release v2", DxPx = 6, DyPx = -10
                    }
                }));

        AngleSharp.Dom.IElement textEl = cut.Find(".bob-chart__annotation-text");
        textEl.TextContent.Should().Be("Release v2");
        textEl.GetAttribute("pointer-events").Should().Be("none");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Text_Annotation_Should_Apply_Pixel_Offsets(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartTextAnnotation<int, double>
                    {
                        X = 1, Y = 1.0, Text = "T", DxPx = 12, DyPx = 0
                    }
                }));

        AngleSharp.Dom.IElement textEl = cut.Find(".bob-chart__annotation-text");
        // The annotation's X attribute should equal the X=1 marker's cx + 12.
        // Reading the first marker keeps the assertion robust to LinearScale's
        // domain-nicing logic.
        double markerCx = double.Parse(cut.Find("circle.bob-line-chart__marker").GetAttribute("cx")!,
            System.Globalization.CultureInfo.InvariantCulture);
        double x = double.Parse(textEl.GetAttribute("x")!, System.Globalization.CultureInfo.InvariantCulture);
        x.Should().BeApproximately(markerCx + 12, 1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Band_Annotation_Should_Render_Rect_Spanning_Domain(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartBandAnnotation<int>
                    {
                        FromX = 3, ToX = 6, FillOpacity = 0.2, Label = "Outage"
                    }
                }));

        cut.FindAll("rect.bob-chart__annotation-band").Should().HaveCount(1);
        AngleSharp.Dom.IElement bandLabel = cut.Find(".bob-chart__annotation-band-label");
        bandLabel.TextContent.Should().Be("Outage");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Band_Annotation_Should_Use_FillOpacity(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartBandAnnotation<int> { FromX = 2, ToX = 4, FillOpacity = 0.42 }
                }));

        AngleSharp.Dom.IElement band = cut.Find("rect.bob-chart__annotation-band");
        band.GetAttribute("fill-opacity").Should().Be("0.42");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Shape_Annotation_Circle_Should_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartShapeAnnotation<int, double>
                    {
                        X = 5, Y = 5.0, Shape = BOBChartAnnotationShape.Circle, SizePx = 16
                    }
                }));

        AngleSharp.Dom.IElement circle = cut.Find("circle.bob-chart__annotation-shape");
        double r = double.Parse(circle.GetAttribute("r")!, System.Globalization.CultureInfo.InvariantCulture);
        r.Should().Be(8);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Shape_Annotation_Square_Should_Render_Rect(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartShapeAnnotation<int, double>
                    {
                        X = 5, Y = 5.0, Shape = BOBChartAnnotationShape.Square, SizePx = 14
                    }
                }));

        cut.FindAll("rect.bob-chart__annotation-shape").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Shape_Annotation_Triangle_Should_Render_Path(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartShapeAnnotation<int, double>
                    {
                        X = 5, Y = 5.0, Shape = BOBChartAnnotationShape.Triangle, SizePx = 12
                    }
                }));

        AngleSharp.Dom.IElement path = cut.Find("path.bob-chart__annotation-shape");
        string d = path.GetAttribute("d") ?? string.Empty;
        d.Should().StartWith("M ").And.EndWith(" Z");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Shape_Annotation_Star_Should_Render_Path(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartShapeAnnotation<int, double>
                    {
                        X = 5, Y = 5.0, Shape = BOBChartAnnotationShape.Star, SizePx = 18
                    }
                }));

        AngleSharp.Dom.IElement path = cut.Find("path.bob-chart__annotation-shape");
        string d = path.GetAttribute("d") ?? string.Empty;
        // 5-point star path = 10 vertices each starting with " L " (after the
        // initial "M "). Counts as 9 " L " separators + closing " Z".
        System.Text.RegularExpressions.Regex.Matches(d, @" L ").Count.Should().Be(9);
        d.Should().EndWith(" Z");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Annotation_Color_Override_Should_Apply(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartShapeAnnotation<int, double>
                    {
                        X = 3, Y = 3.0, Color = "#ff0000"
                    }
                }));

        AngleSharp.Dom.IElement circle = cut.Find("circle.bob-chart__annotation-shape");
        circle.GetAttribute("fill").Should().Be("#ff0000");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Annotation_CssClass_Should_Append_To_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartTextAnnotation<int, double>
                    {
                        X = 5, Y = 5.0, Text = "X", CssClass = "my-custom"
                    }
                }));

        AngleSharp.Dom.IElement textEl = cut.Find(".bob-chart__annotation-text");
        textEl.GetAttribute("class").Should().Contain("my-custom");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Annotations_Mixed_Types_Should_Render_All(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BOBLineChart<int, double>> cut =
            ctx.Render<BOBLineChart<int, double>>(p => p
                .Add(c => c.Series, Series10())
                .Add(c => c.Annotations, new BOBChartAnnotation[]
                {
                    new BOBChartBandAnnotation<int> { FromX = 2, ToX = 4 },
                    new BOBChartTextAnnotation<int, double> { X = 6, Y = 6.0, Text = "T" },
                    new BOBChartShapeAnnotation<int, double> { X = 8, Y = 8.0, Shape = BOBChartAnnotationShape.Diamond },
                }));

        cut.FindAll("rect.bob-chart__annotation-band").Should().HaveCount(1);
        cut.FindAll(".bob-chart__annotation-text").Should().HaveCount(1);
        cut.FindAll("path.bob-chart__annotation-shape").Should().HaveCount(1,
            because: "diamond is a path shape");
    }
}
