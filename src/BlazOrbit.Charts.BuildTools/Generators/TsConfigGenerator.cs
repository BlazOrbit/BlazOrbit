using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Charts.BuildTools.Generators;

/// <summary>
/// Generates a <c>tsconfig.json</c> tuned for the chart-family interop
/// modules: ES2020 target, strict null checks, no emit (esbuild does the
/// emitting), DOM lib for ResizeObserver / Canvas APIs.
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class TsConfigGenerator : IAssetGenerator
{
    public string FileName => "tsconfig.json";
    public string Name => "tsconfig.json";

    public Task<string> GetContent() => Task.FromResult("""
                                                        {
                                                          "compilerOptions": {
                                                            "target": "ES2020",
                                                            "module": "ESNext",
                                                            "moduleResolution": "Bundler",
                                                            "lib": ["ES2020", "DOM", "DOM.Iterable"],
                                                            "strict": true,
                                                            "noEmit": true,
                                                            "esModuleInterop": true,
                                                            "skipLibCheck": true,
                                                            "forceConsistentCasingInFileNames": true
                                                          },
                                                          "include": ["Types/**/*.ts"]
                                                        }
                                                        """);
}