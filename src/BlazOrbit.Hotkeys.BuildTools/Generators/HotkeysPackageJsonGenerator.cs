using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Hotkeys.BuildTools.Generators;

/// <summary>
/// Generates the <c>package.json</c> consumed by the hotkeys esbuild
/// MSBuild target. The project pulls esbuild as a devDependency so the
/// TS-to-JS pipeline runs identically on every machine without relying
/// on a global install.
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class HotkeysPackageJsonGenerator : IAssetGenerator
{
    public string FileName => "package.json";
    public string Name => "esbuild package.json";

    public Task<string> GetContent() => Task.FromResult("""
{
  "name": "blazorbit-hotkeys-js",
  "version": "1.0.0",
  "description": "Hotkeys TypeScript interop bundle for BlazOrbit.Hotkeys. Built into wwwroot/js/ at MSBuild time.",
  "private": true,
  "type": "module",
  "scripts": {
    "build:js": "esbuild Types/Hotkey/HotkeyInterop.ts --bundle --minify --format=esm --target=es2020 --outfile=wwwroot/js/Hotkey/HotkeyInterop.min.js"
  },
  "devDependencies": {
    "esbuild": "latest",
    "typescript": "latest"
  }
}
""");
}
