using System.Diagnostics.CodeAnalysis;

#region RS0041 - BuildRenderTree oblivious reference types (Blazor)

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Notifications.BOBNotificationBell.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

#endregion