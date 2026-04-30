namespace BlazOrbit.Components.Forms.Dropdown;

/// <summary>
/// Provides context information when no results are found in a dropdown search.
/// </summary>
/// <param name="SearchText">The current search text entered by the user.</param>
/// <param name="TotalOptionsCount">The total number of available options.</param>
public readonly record struct NoResultsContext(
    string SearchText,
    int TotalOptionsCount);
