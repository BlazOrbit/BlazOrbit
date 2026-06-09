namespace BlazOrbit.Components;

/// <summary>Pre-defined max-width tier for <c>BOBContainer</c> and <c>BOBSection</c>.</summary>
public enum BOBContainerSize
{
    /// <summary>Narrow (480px) — login forms, compact dialogs.</summary>
    Small = 0,

    /// <summary>Medium (768px) — typical reading column.</summary>
    Medium = 1,

    /// <summary>Large (1024px) — content + sidebar.</summary>
    Large = 2,

    /// <summary>Wide (1280px) — dashboard layouts.</summary>
    Wide = 3,

    /// <summary>Full width (no max).</summary>
    Full = 4
}