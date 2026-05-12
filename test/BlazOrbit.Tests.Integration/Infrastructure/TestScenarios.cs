using BlazOrbit.Tests.Integration.Infrastructure.Contexts;

namespace BlazOrbit.Tests.Integration.Infrastructure;

public sealed record BlazorScenario(
    string Name,
    Func<BlazorTestContextBase> CreateContext);

public class TestScenarios
{
    public static IEnumerable<object[]> All
        =>
        [
            [
                new BlazorScenario(
                    "Server",
                    () => new ServerTestContext())
            ],
            [
                new BlazorScenario(
                    "Wasm",
                    () => new WasmTestContext())
            ]
        ];

    public static IEnumerable<object[]> OnlyServer
        =>
        [
            [
                new BlazorScenario(
                    "Server",
                    () => new ServerTestContext())
            ]
        ];

    public static IEnumerable<object[]> OnlyWasm
        =>
        [
            [
                new BlazorScenario(
                    "Wasm",
                    () => new WasmTestContext())
            ]
        ];
}