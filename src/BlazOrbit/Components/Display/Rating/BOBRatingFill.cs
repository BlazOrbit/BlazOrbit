namespace BlazOrbit.Components.Display;

/// <summary>
/// Fill state of a single rating slot rendered by <see cref="BOBRating"/>.
/// </summary>
public enum BOBRatingFill
{
    /// <summary>Below half the slot's value (renders <c>EmptyIcon</c>).</summary>
    Empty = 0,

    /// <summary>Between 50% and 99% of the slot (renders <c>HalfIcon</c>).</summary>
    Half = 1,

    /// <summary>At or above the slot's threshold (renders <c>FullIcon</c>).</summary>
    Full = 2
}