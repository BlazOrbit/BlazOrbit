// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

#region RS0041 - BuildRenderTree oblivious reference types (Blazor)

// Blazor generates a BuildRenderTree method that references RenderTreeBuilder from the pre-nullable Microsoft.AspNetCore.Components package.
// This causes RS0041 to be raised because the public API file expects all references to be nullable-aware.
// Suppress this warning for the BuildRenderTree method as it is already correctly added in the public API file and cannot be changed to reference
// a nullable-aware type without breaking Blazor's code generation.

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Server.BOBCultureSelector.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

#endregion
