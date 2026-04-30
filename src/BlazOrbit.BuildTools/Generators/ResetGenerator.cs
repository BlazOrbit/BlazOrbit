using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.BuildTools.Generators;

/// <summary>
/// Generates minimal CSS reset. Import order: First (before all other styles).
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class ResetGenerator : IAssetGenerator
{
    public string FileName => "_reset.css";
    public string Name => "CSS Reset";

    public Task<string> GetContent() => Task.FromResult($$"""
/* ========================================
   Minimal CSS Reset
   Auto-generated - Do not edit manually
   ======================================== */

*, *::before, *::after {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
}

html {
    font-size: 16px;
    -webkit-text-size-adjust: 100%;
}

body {
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
}

button, input, select, textarea {
    font: inherit;
    color: inherit;
    background: transparent;
    border: none;
    outline: none;
}

button { cursor: pointer; }
a { color: inherit; text-decoration: none; }
img, svg, video { display: block; max-inline-size: 100%; }
ul, ol { list-style: none; }
[hidden] { display: none !important; }
""");
}