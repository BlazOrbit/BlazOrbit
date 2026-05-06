using BlazOrbit.Components;
using FluentAssertions;

namespace BlazOrbit.Tests.Integration.Tests.Library;

[Trait("Library", "BOBPerformanceService")]
public class BOBPerformanceServiceTests
{
    [Fact]
    public void RecordRenderTreeBuild_Should_Create_New_Metrics()
    {
        BOBPerformanceService sut = new();

        sut.RecordRenderTreeBuild("BOBButton", 12.5);

        BOBComponentMetrics? metrics = sut.Get("BOBButton");
        metrics.Should().NotBeNull();
        metrics!.RenderCount.Should().Be(1);
        metrics.TotalRenderTreeBuildTimeMs.Should().Be(12.5);
        metrics.LastRenderTreeBuildTimeMs.Should().Be(12.5);
    }

    [Fact]
    public void RecordRenderTreeBuild_Should_Update_Existing_Metrics()
    {
        BOBPerformanceService sut = new();
        sut.RecordRenderTreeBuild("BOBButton", 10.0);

        sut.RecordRenderTreeBuild("BOBButton", 20.0);

        BOBComponentMetrics? metrics = sut.Get("BOBButton");
        metrics.Should().NotBeNull();
        metrics!.RenderCount.Should().Be(2);
        metrics.TotalRenderTreeBuildTimeMs.Should().Be(30.0);
        metrics.LastRenderTreeBuildTimeMs.Should().Be(20.0);
        metrics.AverageRenderTreeBuildTimeMs.Should().Be(15.0);
    }

    [Fact]
    public void RecordInit_Should_Set_InitTime()
    {
        BOBPerformanceService sut = new();

        sut.RecordInit("BOBButton", 5.0);

        sut.Get("BOBButton")!.InitTimeMs.Should().Be(5.0);
    }

    [Fact]
    public void RecordParametersSet_Should_Create_And_Accumulate()
    {
        BOBPerformanceService sut = new();

        sut.RecordParametersSet("BOBButton", 8.0);
        sut.RecordParametersSet("BOBButton", 12.0);

        BOBComponentMetrics? metrics = sut.Get("BOBButton");
        metrics!.ParametersSetCount.Should().Be(2);
        metrics.TotalParametersSetTimeMs.Should().Be(20.0);
        metrics.AverageParametersSetTimeMs.Should().Be(10.0);
    }

    [Fact]
    public void Get_Should_Return_Null_For_Unknown_Component()
    {
        BOBPerformanceService sut = new();
        sut.Get("Unknown").Should().BeNull();
    }

    [Fact]
    public void GetAll_Should_Order_By_TotalRenderTreeBuildTimeMs_Descending()
    {
        BOBPerformanceService sut = new();
        sut.RecordRenderTreeBuild("BOBA", 30.0);
        sut.RecordRenderTreeBuild("BOBB", 10.0);
        sut.RecordRenderTreeBuild("BOBC", 50.0);

        var all = sut.GetAll().ToList();

        all.Should().HaveCount(3);
        all[0].ComponentType.Should().Be("BOBC");
        all[1].ComponentType.Should().Be("BOBA");
        all[2].ComponentType.Should().Be("BOBB");
    }

    [Fact]
    public void Reset_Should_Clear_All_Metrics()
    {
        BOBPerformanceService sut = new();
        sut.RecordRenderTreeBuild("BOBButton", 10.0);

        sut.Reset();

        sut.Get("BOBButton").Should().BeNull();
        sut.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void MetricsUpdated_Should_Fire_On_Every_Operation()
    {
        BOBPerformanceService sut = new();
        int callCount = 0;
        sut.MetricsUpdated += () => callCount++;

        sut.RecordRenderTreeBuild("A", 1.0);
        sut.RecordInit("B", 1.0);
        sut.RecordParametersSet("C", 1.0);
        sut.Reset();

        callCount.Should().Be(4);
    }
}
