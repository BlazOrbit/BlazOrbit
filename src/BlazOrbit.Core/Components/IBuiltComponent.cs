namespace BlazOrbit.Components;

/// <summary>
/// Contract implemented by every BOB component base so that the attributes builder can invite the
/// component to contribute extra data-attributes and CSS custom properties to the root element.
///
/// Invocation order: these hooks are called *before* the framework writes its own
/// `data-bob-*` attributes and `--bob-inline-*` variables, so any key a component sets that
/// collides with a framework-owned key will be overwritten. This is intentional — the framework
/// owns the contract exposed by <c>FeatureDefinitions</c> (read via the public
/// <c>BOBStylingKeys</c> facade), and components may only contribute *additional* keys.
/// </summary>
internal interface IBuiltComponent
{
    void BuildComponentCssVariables(Dictionary<string, string> cssVariables);

    void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes);
}