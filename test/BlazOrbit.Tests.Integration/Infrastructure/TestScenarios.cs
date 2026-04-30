using BlazOrbit.Tests.Integration.Infrastructure.Contexts;

namespace BlazOrbit.Tests.Integration.Infrastructure;

public sealed record BlazorScenario(
    string Name,
    Func<BlazorTestContextBase> CreateContext);

public class TestScenarios
{
    public static IEnumerable<object[]> All
    => [
        new object[]
        {
            new BlazorScenario(
                "Server",
                () => new ServerTestContext())
        },
        new object[]
        {
            new BlazorScenario(
                "Wasm",
                () => new WasmTestContext())
        }
    ];

    public static IEnumerable<object[]> OnlyServer
    => [
        new object[]
        {
            new BlazorScenario(
                "Server",
                () => new ServerTestContext())
        }
    ];

    public static IEnumerable<object[]> OnlyWasm
        => [
        new object[]
        {
            new BlazorScenario(
                "Wasm",
                () => new WasmTestContext())
        }
    ];
}