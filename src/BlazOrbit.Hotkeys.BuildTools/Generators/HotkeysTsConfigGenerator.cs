using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Hotkeys.BuildTools.Generators;

/// <summary>
/// Generates a <c>tsconfig.json</c> tuned for the hotkeys interop
/// module: ES2020 target, strict null checks, no emit (esbuild does the
/// emitting), DOM lib for keyboard events.
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class HotkeysTsConfigGenerator : IAssetGenerator
{
    public string FileName => "tsconfig.json";
    public string Name => "tsconfig.json";

    public Task<string> GetContent() => Task.FromResult("""
                                                        {
                                                          "compilerOptions": {
                                                            "target": "ES2020",
                                                            "module": "ESNext",
                                                            "moduleResolution": "Bundler",
                                                            "lib": ["ES2020", "DOM"],
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