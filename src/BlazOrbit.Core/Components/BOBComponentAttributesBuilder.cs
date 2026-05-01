using BlazOrbit.Components;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;

namespace BlazOrbit.Abstractions;

internal sealed class BOBComponentAttributesBuilder
{
    [Flags]
    private enum ComponentFeatures : uint
    {
        None = 0,
        Variant = 1u << 0,
        Size = 1u << 1,
        Density = 1u << 2,
        FullWidth = 1u << 3,
        Loading = 1u << 4,
        Error = 1u << 5,
        Disabled = 1u << 6,
        Active = 1u << 7,
        ReadOnly = 1u << 8,
        Required = 1u << 9,
        Prefix = 1u << 10,
        Suffix = 1u << 11,
        Shadow = 1u << 12,
        Ripple = 1u << 13,
        Color = 1u << 14,
        BackgroundColor = 1u << 15,
        Border = 1u << 16,
        Transitions = 1u << 17,
        InputFamily = 1u << 18,
        PickerFamily = 1u << 19,
        DataCollectionFamily = 1u << 20,
        BuiltComponent = 1u << 21,
        Elevation = 1u << 22,

        VolatileMask =
            Active | Disabled | Loading | Error | ReadOnly | Required | FullWidth,
    }

    private readonly record struct TypeInfo(string ComponentName, ComponentFeatures Features);

    private static readonly ConcurrentDictionary<Type, TypeInfo> _typeInfoCache = new();

    private static TypeInfo GetTypeInfo(Type type)
        => _typeInfoCache.GetOrAdd(type, ComputeTypeInfo);

    private static TypeInfo ComputeTypeInfo(Type type)
    {
        ComponentFeatures flags = ComponentFeatures.None;

        if (typeof(IVariantComponent).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Variant;
        }

        if (typeof(IHasSize).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Size;
        }

        if (typeof(IHasDensity).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Density;
        }

        if (typeof(IHasFullWidth).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.FullWidth;
        }

        if (typeof(IHasLoading).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Loading;
        }

        if (typeof(IHasError).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Error;
        }

        if (typeof(IHasDisabled).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Disabled;
        }

        if (typeof(IHasActive).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Active;
        }

        if (typeof(IHasReadOnly).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.ReadOnly;
        }

        if (typeof(IHasRequired).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Required;
        }

        if (typeof(IHasPrefix).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Prefix;
        }

        if (typeof(IHasSuffix).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Suffix;
        }

        if (typeof(IHasShadow).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Shadow;
        }

        if (typeof(IHasElevation).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Elevation;
        }

        if (typeof(IHasRipple).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Ripple;
        }

        if (typeof(IHasColor).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Color;
        }

        if (typeof(IHasBackgroundColor).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.BackgroundColor;
        }

        if (typeof(IHasBorder).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Border;
        }

        if (typeof(IHasTransitions).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.Transitions;
        }

        if (typeof(IInputFamilyComponent).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.InputFamily;
        }

        if (typeof(IPickerFamilyComponent).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.PickerFamily;
        }

        if (typeof(IDataCollectionFamilyComponent).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.DataCollectionFamily;
        }

        if (typeof(IBuiltComponent).IsAssignableFrom(type))
        {
            flags |= ComponentFeatures.BuiltComponent;
        }

        return new TypeInfo(ToKebabCaseComponentName(type.Name), flags);
    }

    private static string BoolToAttr(bool value) => value ? "true" : "false";

    private static int ComputeStyleFingerprint(ComponentBase component)
    {
        ComponentFeatures flags = GetTypeInfo(component.GetType()).Features;
        HashCode hc = new();
        hc.Add(component.GetType());

        // IBuiltComponent contributes via callbacks whose output we can't fingerprint
        // safely. Fold the component's own identity hash so two different built
        // components don't collide; correctness still holds because a built component
        // that mutates its data-attrs in `BuildComponentDataAttributes` would also
        // mutate one of the underlying parameters → fingerprint diverges. If a consumer
        // emits time-varying attributes, they must override and bypass this cache.
        if ((flags & ComponentFeatures.BuiltComponent) != 0)
        {
            hc.Add(RuntimeHelpers.GetHashCode(component));
        }

        if ((flags & ComponentFeatures.Variant) != 0)
        {
            hc.Add(((IVariantComponent)component).CurrentVariant);
        }

        if ((flags & ComponentFeatures.Size) != 0)
        {
            hc.Add(((IHasSize)component).Size);
        }

        if ((flags & ComponentFeatures.Density) != 0)
        {
            hc.Add(((IHasDensity)component).Density);
        }

        if ((flags & ComponentFeatures.FullWidth) != 0)
        {
            hc.Add(((IHasFullWidth)component).FullWidth);
        }

        if ((flags & ComponentFeatures.Loading) != 0)
        {
            hc.Add(((IHasLoading)component).Loading);
        }

        if ((flags & ComponentFeatures.Error) != 0)
        {
            hc.Add(((IHasError)component).IsError);
        }

        if ((flags & ComponentFeatures.Disabled) != 0)
        {
            hc.Add(((IHasDisabled)component).IsDisabled);
        }

        if ((flags & ComponentFeatures.Active) != 0)
        {
            hc.Add(((IHasActive)component).IsActive);
        }

        if ((flags & ComponentFeatures.ReadOnly) != 0)
        {
            hc.Add(((IHasReadOnly)component).IsReadOnly);
        }

        if ((flags & ComponentFeatures.Required) != 0)
        {
            hc.Add(((IHasRequired)component).IsRequired);
        }

        if ((flags & ComponentFeatures.Prefix) != 0)
        {
            IHasPrefix p = (IHasPrefix)component;
            hc.Add(p.PrefixColor);
            hc.Add(p.PrefixBackgroundColor);
        }

        if ((flags & ComponentFeatures.Suffix) != 0)
        {
            IHasSuffix s = (IHasSuffix)component;
            hc.Add(s.SuffixColor);
            hc.Add(s.SuffixBackgroundColor);
        }

        if ((flags & ComponentFeatures.Shadow) != 0)
        {
            hc.Add(((IHasShadow)component).Shadow);
        }

        if ((flags & ComponentFeatures.Elevation) != 0)
        {
            hc.Add(((IHasElevation)component).Elevation);
        }

        if ((flags & ComponentFeatures.Ripple) != 0)
        {
            IHasRipple r = (IHasRipple)component;
            hc.Add(r.DisableRipple);
            hc.Add(r.RippleColor);
            hc.Add(r.RippleDurationMs);
        }

        if ((flags & ComponentFeatures.Color) != 0)
        {
            hc.Add(((IHasColor)component).Color);
        }

        if ((flags & ComponentFeatures.BackgroundColor) != 0)
        {
            hc.Add(((IHasBackgroundColor)component).BackgroundColor);
        }

        if ((flags & ComponentFeatures.Border) != 0)
        {
            hc.Add(((IHasBorder)component).Border);
        }

        if ((flags & ComponentFeatures.Transitions) != 0)
        {
            hc.Add(((IHasTransitions)component).Transitions);
        }

        return hc.ToHashCode();
    }

    private string? _originalUserStyles;
    private string? _lastStyleString;
    private readonly Dictionary<string, string> _cssVariables = [];
    private readonly StringBuilder _styleBuilder = new();
    public Dictionary<string, object> ComputedAttributes { get; } = [];

    // Skip the full ComputedAttributes/cssVariables rebuild when none of the
    // style-affecting inputs changed. `additionalAttributes` is captured by reference
    // identity (the parent typically reuses the same dictionary unless it actually
    // mutates the captured-attributes set); the rest of the inputs are folded into a
    // single hash. PatchVolatileAttributes still runs every render via BuildRenderTree
    // so volatile state changes (Disabled/Loading/etc.) are picked up regardless.
    private bool _hasBuiltOnce;
    private int _lastFingerprint;
    private IReadOnlyDictionary<string, object>? _lastAdditionalAttributes;

    /// <summary>
    /// Signals whether the most recent <see cref="BuildStyles"/> call hit the
    /// fingerprint cache and returned without rebuilding. Consumed by
    /// <see cref="BOBInputComponentBase{TValue}"/>'s <c>ShouldRender</c> echo-guard so
    /// the input can suppress redundant render-tree builds when the round-trip from
    /// <c>ValueChanged</c> echoes the same Value back without any style change.
    /// </summary>
    public bool LastBuildSkipped { get; private set; }

    public void BuildStyles(
        ComponentBase component,
        IReadOnlyDictionary<string, object>? additionalAttributes)
    {
        // IBuiltComponent components emit their own data-attrs / css-vars from
        // `BuildComponentDataAttributes` / `BuildComponentCssVariables` callbacks
        // that read arbitrary component-private state. We can't fingerprint that
        // safely, so the fast-path cache is disabled for them. Non-built components
        // (the vast majority — UIButton, UICard, etc.) still benefit.
        ComponentFeatures flags = GetTypeInfo(component.GetType()).Features;
        bool cacheEligible = (flags & ComponentFeatures.BuiltComponent) == 0;

        if (cacheEligible)
        {
            int fingerprint = ComputeStyleFingerprint(component);
            if (_hasBuiltOnce
                && fingerprint == _lastFingerprint
                && ReferenceEquals(additionalAttributes, _lastAdditionalAttributes))
            {
                LastBuildSkipped = true;
                return;
            }

            _lastFingerprint = fingerprint;
            _lastAdditionalAttributes = additionalAttributes;
            _hasBuiltOnce = true;
        }
        else
        {
            // Built components opt out: invalidate the cache so a future state where
            // the component stops being built (unlikely, but defensive) re-fingerprints
            // from scratch.
            _hasBuiltOnce = false;
        }

        LastBuildSkipped = false;

        ComputedAttributes.Clear();
        if (additionalAttributes != null)
        {
            foreach (KeyValuePair<string, object> kv in additionalAttributes)
            {
                ComputedAttributes[kv.Key] = kv.Value;
            }
        }

        _originalUserStyles = ComputedAttributes.TryGetValue("style", out object? style)
            ? style?.ToString()
            : null;

        Dictionary<string, string> cssVariables = _cssVariables;
        cssVariables.Clear();

        TypeInfo typeInfo = GetTypeInfo(component.GetType());

        // Component-supplied keys are contributed first so that the framework-owned keys below
        // overwrite any collision. Components may only add *new* data-attrs / inline vars.
        if ((flags & ComponentFeatures.BuiltComponent) != 0)
        {
            IBuiltComponent builtFirst = (IBuiltComponent)component;
            builtFirst.BuildComponentDataAttributes(ComputedAttributes);
            builtFirst.BuildComponentCssVariables(cssVariables);
        }

        ComputedAttributes[FeatureDefinitions.DataAttributes.Component] = typeInfo.ComponentName;

        if ((flags & ComponentFeatures.Variant) != 0)
        {
            IVariantComponent variantComponent = (IVariantComponent)component;
            ComputedAttributes[FeatureDefinitions.DataAttributes.Variant] = variantComponent.CurrentVariant.NameLower;
        }

        if ((flags & ComponentFeatures.InputFamily) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.InputBase] = "true";
        }

        if ((flags & ComponentFeatures.PickerFamily) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.PickerBase] = "true";
        }

        if ((flags & ComponentFeatures.DataCollectionFamily) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.DataCollectionBase] = "true";
        }

        if ((flags & ComponentFeatures.Size) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Size] = ((IHasSize)component).Size.ToString().ToLowerInvariant();
        }

        if ((flags & ComponentFeatures.Density) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Density] = ((IHasDensity)component).Density.ToString().ToLowerInvariant();
        }

        if ((flags & ComponentFeatures.FullWidth) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth] = BoolToAttr(((IHasFullWidth)component).FullWidth);
        }

        if ((flags & ComponentFeatures.Loading) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Loading] = BoolToAttr(((IHasLoading)component).Loading);
        }

        if ((flags & ComponentFeatures.Error) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Error] = BoolToAttr(((IHasError)component).IsError);
        }

        if ((flags & ComponentFeatures.Disabled) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled] = BoolToAttr(((IHasDisabled)component).IsDisabled);
        }

        if ((flags & ComponentFeatures.Active) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Active] = BoolToAttr(((IHasActive)component).IsActive);
        }

        if ((flags & ComponentFeatures.ReadOnly) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly] = BoolToAttr(((IHasReadOnly)component).IsReadOnly);
        }

        if ((flags & ComponentFeatures.Required) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Required] = BoolToAttr(((IHasRequired)component).IsRequired);
        }

        if ((flags & ComponentFeatures.Prefix) != 0)
        {
            IHasPrefix prefix = (IHasPrefix)component;
            if (prefix.PrefixColor != null)
            {
                cssVariables[FeatureDefinitions.InlineVariables.PrefixColor] = prefix.PrefixColor;
            }

            if (prefix.PrefixBackgroundColor != null)
            {
                cssVariables[FeatureDefinitions.InlineVariables.PrefixBackgroundColor] = prefix.PrefixBackgroundColor;
            }
        }

        if ((flags & ComponentFeatures.Suffix) != 0)
        {
            IHasSuffix suffix = (IHasSuffix)component;
            if (suffix.SuffixColor != null)
            {
                cssVariables[FeatureDefinitions.InlineVariables.SuffixColor] = suffix.SuffixColor;
            }

            if (suffix.SuffixBackgroundColor != null)
            {
                cssVariables[FeatureDefinitions.InlineVariables.SuffixBackgroundColor] = suffix.SuffixBackgroundColor;
            }
        }

        if ((flags & ComponentFeatures.Shadow) != 0)
        {
            IHasShadow shadow = (IHasShadow)component;
            if (shadow.Shadow != null)
            {
                ComputedAttributes[FeatureDefinitions.DataAttributes.Shadow] = "true";
                cssVariables[FeatureDefinitions.InlineVariables.Shadow] = shadow.Shadow.ToCss();
            }
        }

        if ((flags & ComponentFeatures.Elevation) != 0)
        {
            IHasElevation elevation = (IHasElevation)component;
            if (elevation.Elevation is int rawLevel)
            {
                int level = Math.Clamp(rawLevel, 0, 24);
                ComputedAttributes[FeatureDefinitions.DataAttributes.Elevation] =
                    level.ToString(System.Globalization.CultureInfo.InvariantCulture);
                cssVariables[FeatureDefinitions.InlineVariables.ElevationTint] =
                    BOBElevationPresets.SurfaceTintPercent(level).ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";

                // Shadow precedence: an explicit IHasShadow.Shadow already populated
                // --bob-inline-shadow above. Only emit the elevation-derived shadow when
                // no explicit shadow was provided.
                if (!cssVariables.ContainsKey(FeatureDefinitions.InlineVariables.Shadow))
                {
                    ComputedAttributes[FeatureDefinitions.DataAttributes.Shadow] = "true";
                    cssVariables[FeatureDefinitions.InlineVariables.Shadow] = BOBShadowPresets.Elevation(level).ToCss();
                }
            }
        }

        if ((flags & ComponentFeatures.Ripple) != 0)
        {
            // Ripple state is owned by the JS behavior (BOBComponentJsBehaviorBuilder reads
            // DisableRipple/RippleColor/RippleDurationMs from the IHasRipple instance directly).
            // No DOM data-attribute needed.
            IHasRipple ripple = (IHasRipple)component;
            if (!ripple.DisableRipple)
            {
                if (ripple.RippleColor != null)
                {
                    cssVariables[FeatureDefinitions.InlineVariables.RippleColor] = ripple.RippleColor;
                }

                if (ripple.RippleDurationMs.HasValue)
                {
                    cssVariables[FeatureDefinitions.InlineVariables.RippleDuration] = $"{ripple.RippleDurationMs}ms";
                }
            }
        }

        if ((flags & ComponentFeatures.Color) != 0)
        {
            IHasColor color = (IHasColor)component;
            if (color.Color != null)
            {
                cssVariables[FeatureDefinitions.InlineVariables.Color] = color.Color;
            }
        }

        if ((flags & ComponentFeatures.BackgroundColor) != 0)
        {
            IHasBackgroundColor bg = (IHasBackgroundColor)component;
            if (bg.BackgroundColor != null)
            {
                cssVariables[FeatureDefinitions.InlineVariables.BackgroundColor] = bg.BackgroundColor;
            }
        }

        if ((flags & ComponentFeatures.Border) != 0)
        {
            IHasBorder hasBorder = (IHasBorder)component;
            if (hasBorder.Border != null)
            {
                BorderCssValues values = hasBorder.Border.GetCssValues();
                if (values.All != null)
                {
                    cssVariables[FeatureDefinitions.InlineVariables.Border] = values.All;
                }

                if (values.Top != null)
                {
                    cssVariables[FeatureDefinitions.InlineVariables.BorderTop] = values.Top;
                }

                if (values.Right != null)
                {
                    cssVariables[FeatureDefinitions.InlineVariables.BorderRight] = values.Right;
                }

                if (values.Bottom != null)
                {
                    cssVariables[FeatureDefinitions.InlineVariables.BorderBottom] = values.Bottom;
                }

                if (values.Left != null)
                {
                    cssVariables[FeatureDefinitions.InlineVariables.BorderLeft] = values.Left;
                }

                if (values.Radius != null)
                {
                    cssVariables[FeatureDefinitions.InlineVariables.BorderRadius] = values.Radius;
                }
            }
        }

        if ((flags & ComponentFeatures.Transitions) != 0)
        {
            IHasTransitions transitions = (IHasTransitions)component;
            if (transitions.Transitions?.HasTransitions == true)
            {
                foreach (KeyValuePair<string, string> kv in transitions.Transitions.GetCssVariables())
                {
                    cssVariables[kv.Key] = kv.Value;
                }

                ComputedAttributes[FeatureDefinitions.DataAttributes.Transitions] = transitions.Transitions.GetDataAttributeValue();
            }
        }

        BuildInlineStyles(cssVariables);
    }

    public void PatchVolatileAttributes(ComponentBase component)
    {
        ComponentFeatures flags = GetTypeInfo(component.GetType()).Features;

        if ((flags & (ComponentFeatures.VolatileMask | ComponentFeatures.BuiltComponent)) == 0)
        {
            return;
        }

        // Component-supplied data attributes first so that volatile framework state (below)
        // wins on any key collision. See IBuiltComponent XML doc for the ordering contract.
        if ((flags & ComponentFeatures.BuiltComponent) != 0)
        {
            ((IBuiltComponent)component).BuildComponentDataAttributes(ComputedAttributes);
        }

        if ((flags & ComponentFeatures.Active) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Active] = BoolToAttr(((IHasActive)component).IsActive);
        }

        if ((flags & ComponentFeatures.Disabled) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled] = BoolToAttr(((IHasDisabled)component).IsDisabled);
        }

        if ((flags & ComponentFeatures.Loading) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Loading] = BoolToAttr(((IHasLoading)component).Loading);
        }

        if ((flags & ComponentFeatures.Error) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Error] = BoolToAttr(((IHasError)component).IsError);
        }

        if ((flags & ComponentFeatures.ReadOnly) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly] = BoolToAttr(((IHasReadOnly)component).IsReadOnly);
        }

        if ((flags & ComponentFeatures.Required) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Required] = BoolToAttr(((IHasRequired)component).IsRequired);
        }

        if ((flags & ComponentFeatures.FullWidth) != 0)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth] = BoolToAttr(((IHasFullWidth)component).FullWidth);
        }
    }

    private static string ToKebabCaseComponentName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        int tickIndex = value.IndexOf('`');
        if (tickIndex != -1)
        {
            value = value[..tickIndex];
        }

        // Ordinal (case-sensitive): only strip the canonical "BOB" prefix in uppercase. A type
        // like `BuiltInPopup` is not a framework component and must not be mutilated to
        // `lt-in-popup`; a consumer-defined `BuiCustom` is left alone to avoid namespace collision.
        if (value.StartsWith("BOB", StringComparison.Ordinal))
        {
            value = value[3..];
        }

        StringBuilder sb = new();
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsUpper(c) && i > 0 && value[i - 1] != '-')
            {
                sb.Append('-');
            }

            sb.Append(char.ToLower(c));
        }

        return sb.ToString();
    }

    private void BuildInlineStyles(Dictionary<string, string> cssVariables)
    {
        _styleBuilder.Clear();

        bool first = true;
        foreach (KeyValuePair<string, string> kv in cssVariables)
        {
            if (!first)
            {
                _styleBuilder.Append("; ");
            }

            _styleBuilder.Append(kv.Key).Append(": ").Append(kv.Value);
            first = false;
        }

        bool hasUserStyles = !string.IsNullOrWhiteSpace(_originalUserStyles);
        if (hasUserStyles)
        {
            if (_styleBuilder.Length > 0)
            {
                _styleBuilder.Append("; ");
            }

            _styleBuilder.Append(_originalUserStyles);
        }

        if (_styleBuilder.Length > 0)
        {
            if (!StyleBuilderMatchesCached())
            {
                _lastStyleString = _styleBuilder.ToString();
            }

            ComputedAttributes["style"] = _lastStyleString!;
        }
        else
        {
            _lastStyleString = null;
            ComputedAttributes.Remove("style");
        }
    }

    private bool StyleBuilderMatchesCached()
    {
        if (_lastStyleString is null || _lastStyleString.Length != _styleBuilder.Length)
        {
            return false;
        }

        for (int i = 0; i < _styleBuilder.Length; i++)
        {
            if (_styleBuilder[i] != _lastStyleString[i])
            {
                return false;
            }
        }

        return true;
    }
}