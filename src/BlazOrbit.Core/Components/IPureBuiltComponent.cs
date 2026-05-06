namespace BlazOrbit.Components;

/// <summary>
/// Refined opt-in for components that contribute extra data-attributes and CSS custom properties
/// to the root <c>&lt;bob-component&gt;</c> element <em>and</em> guarantee their hooks read only
/// component <see cref="Microsoft.AspNetCore.Components.ParameterAttribute"/> properties or
/// pure getters derived from them. The marker promises:
/// <list type="bullet">
///   <item>No internal field reads (<c>_isFocused</c>, <c>_isDirty</c>, timers, counters, …).</item>
///   <item>No reads of mutable shared state (services, captured state objects).</item>
///   <item>Output is a pure function of the parameter values.</item>
/// </list>
///
/// <para>
/// Components that satisfy this contract participate in the style-fingerprint cache:
/// <c>BOBComponentAttributesBuilder</c> folds the hook contributions into the fingerprint, so
/// repeated <c>BuildStyles</c> calls with the same parameters short-circuit.
/// </para>
///
/// <para>
/// If a component breaks the contract — for example by introducing an internal focus flag —
/// drop the marker and revert to plain <see cref="IBuiltComponent"/>. The cache otherwise freezes
/// the stale value and the component renders out of date.
/// </para>
///
/// <para>
/// Pure inherits from <c>IBuiltComponent</c>, so the standard hook signatures and default
/// no-op virtuals on the base classes still apply. Declaring this marker on a derived component
/// is sufficient to opt in.
/// </para>
/// </summary>
public interface IPureBuiltComponent : IBuiltComponent
{
}
