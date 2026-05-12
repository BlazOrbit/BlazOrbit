using System.Diagnostics.CodeAnalysis;

#region RS0041 - BuildRenderTree oblivious reference types (Blazor)

// Blazor generates a BuildRenderTree method that references RenderTreeBuilder from the
// pre-nullable Microsoft.AspNetCore.Components package. RS0041 fires because the public
// API file expects nullable-aware references everywhere; the entry is correctly marked
// with the `~override` annotation and cannot be changed without breaking Blazor codegen.

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Hotkeys.BOBHotkeyHost.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

#endregion