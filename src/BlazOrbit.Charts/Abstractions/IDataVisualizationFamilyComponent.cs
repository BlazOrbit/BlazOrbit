namespace BlazOrbit.Charts.Abstractions;

/// <summary>
/// Marker interface that identifies a component as part of the BlazOrbit
/// data-visualization family (charts, sparklines, gauges).
/// <para>
/// Implementing types receive the <c>data-bob-data-visualization-base</c>
/// attribute on their <c>&lt;bob-component&gt;</c> root, which in turn unlocks
/// the family-shared CSS rules (axis grids, legend layout, tooltip floating)
/// shipped by <c>BlazOrbit.Charts</c>.
/// </para>
/// <para>
/// This is parallel to <c>IDataCollectionFamilyComponent</c> from the main
/// library (which scopes <c>BOBDataGrid</c> / <c>BOBDataCards</c>); the two
/// families are deliberately distinct because tabular and chart layouts share
/// no CSS surface.
/// </para>
/// </summary>
public interface IDataVisualizationFamilyComponent
{
}
