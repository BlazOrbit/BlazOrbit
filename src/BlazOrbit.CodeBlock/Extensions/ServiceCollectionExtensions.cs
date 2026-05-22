namespace Microsoft.Extensions.DependencyInjection;

/// <summary>DI registration helpers for the <c>BlazOrbit.CodeBlock</c> package.</summary>
public static class CodeBlockServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <c>BlazOrbit.CodeBlock</c> component family. Currently a no-op because
    /// <see cref="BlazOrbit.CodeBlock.Components.BOBCodeBlock"/> only depends on services already registered by
    /// <c>AddBlazOrbit()</c> (theme, clipboard JS interop, localization). The call is kept as
    /// a stable extension point so future optional services (e.g. custom highlighters) can be
    /// wired here without breaking consumers.
    /// </summary>
    public static IServiceCollection AddBlazOrbitCodeBlock(this IServiceCollection services)
    {
        // Idempotent - nothing to register today.
        return services;
    }
}
