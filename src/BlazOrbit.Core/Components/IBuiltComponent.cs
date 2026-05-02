namespace BlazOrbit.Components;

/// <summary>
/// Opt-in contract for components that contribute extra data-attributes and CSS custom properties
/// to the root <c>&lt;bob-component&gt;</c> element. Implementing this interface signals two things
/// to <c>BOBComponentAttributesBuilder</c>:
/// <list type="number">
///   <item>The component participates in the <c>BuildComponent*</c> hooks; the builder will call
///   them on every <c>BuildStyles</c> and on the volatile re-patch path.</item>
///   <item>The component opts out of the style-fingerprint cache. Hooks may read state opaque to
///   the fingerprint (timers, derived counters, etc.), so the builder rebuilds on every
///   parameter set instead of attempting to reuse a cached attribute set.</item>
/// </list>
///
/// <para>
/// Invocation order: hooks are called <em>before</em> the framework writes its own
/// <c>data-bob-*</c> attributes and <c>--bob-inline-*</c> variables, so any key a component sets
/// that collides with a framework-owned key will be overwritten. The framework owns the contract
/// exposed by <c>FeatureDefinitions</c>; components may only contribute <em>additional</em> keys.
/// </para>
///
/// <para>
/// Default no-op virtuals live on <c>BOBComponentBase</c> and <c>BOBInputComponentBase&lt;TValue&gt;</c>;
/// declaring <c>IBuiltComponent</c> on a derived component is sufficient to opt in — the inherited
/// virtuals satisfy the interface contract until the component overrides one of them.
/// </para>
/// </summary>
public interface IBuiltComponent
{
    /// <summary>
    /// Contributes additional CSS custom properties to the inline <c>style</c> attribute. Keys
    /// that collide with framework-owned <c>--bob-inline-*</c> variables will be overwritten.
    /// </summary>
    void BuildComponentCssVariables(Dictionary<string, string> cssVariables);

    /// <summary>
    /// Contributes additional <c>data-*</c> attributes to the root element. Keys that collide
    /// with framework-owned <c>data-bob-*</c> attributes will be overwritten.
    /// </summary>
    void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes);
}