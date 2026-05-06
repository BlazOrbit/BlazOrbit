namespace BlazOrbit.Components;

/// <summary>
/// Central definition of all CSS-related constants used by the BOB component library. Organized by
/// purpose and usage context.
///
/// INTERNAL: this surface is part of the library's private CSS/DOM contract and is subject
/// to change between minor versions. Consumers that need to read or compose a small set of
/// framework-driven keys must use the public facade <c>BlazOrbit.Components.BOBStylingKeys</c>.
/// </summary>
internal static class FeatureDefinitions
{
    public static class ComponentVariables
    {
        /// <summary>
        /// Density-related variables set by [data-bob-density]. Affects spacing between elements
        /// via gap.
        /// </summary>
        public static class Density
        {
            public static string Multiplier => "--bob-density-multiplier";
        }

        /// <summary>
        /// Size-related variables set by [data-bob-size]. Only the multiplier is set globally;
        /// components use it in their isolated CSS.
        /// </summary>
        public static class Size
        {
            /// <summary>
            /// Scale factor for size calculations. Small=0.85, Medium=1, Large=1.25 Components
            /// multiply their dimensions by this value.
            /// </summary>
            public const string Multiplier = "--bob-size-multiplier";
        }
    }

    public static class CssClasses
    {
        /// <summary>
        /// Ripple effect element class.
        /// </summary>
        public const string Ripple = "bob-ripple";

        /// <summary>
        /// Visually hidden but accessible to screen readers.
        /// </summary>
        public const string SrOnly = "sr-only";

        /// <summary>
        /// Utility class variant of <see cref="DataAttributes.Scrollbars"/> — place on any wrapper to scope
        /// branded scrollbar styling.
        /// </summary>
        public const string Scrollbars = "bob-scrollbars";

        /// <summary>
        /// Input family component class names.
        /// </summary>
        public static class Input
        {
            public const string Wrapper = "bob-input__wrapper";
            public const string Field = "bob-input__field";
            public const string Label = "bob-input__label";
            public const string Required = "bob-input__required";
            public const string HelperText = "bob-input__helper-text";
            public const string Validation = "bob-input__validation";

            public const string AddonPrefix = "bob-input__addon--prefix";
            public const string AddonSuffix = "bob-input__addon--suffix";

            public const string Outline = "bob-input__outline";
            public const string OutlineLeading = "bob-input__outline-leading";
            public const string OutlineNotch = "bob-input__outline-notch";
            public const string OutlineTrailing = "bob-input__outline-trailing";
        }

        /// <summary>
        /// Data-collection family component class names.
        /// </summary>
        public static class DataCollection
        {
            public const string Toolbar = "bob-dc__toolbar";
            public const string ToolbarSpacer = "bob-dc__toolbar-spacer";
            public const string Filter = "bob-dc__filter";
            public const string SelectionInfo = "bob-dc__selection-info";
            public const string PageSize = "bob-dc__page-size";
            public const string Pagination = "bob-dc__pagination";
            public const string PaginationInfo = "bob-dc__pagination-info";
            public const string PaginationControls = "bob-dc__pagination-controls";
            public const string Checkbox = "bob-dc__checkbox";
            public const string Empty = "bob-dc__empty";
            public const string EmptyIcon = "bob-dc__empty-icon";
            public const string EmptyText = "bob-dc__empty-text";
            public const string Loading = "bob-dc__loading";
        }

        /// <summary>
        /// Picker family component class names.
        /// </summary>
        public static class Picker
        {
            public const string Row = "bob-picker__row";
            public const string Title = "bob-picker__title";
            public const string Grid = "bob-picker__grid";
            public const string Cell = "bob-picker__cell";
            public const string CellSelected = "bob-picker__cell--selected";
            public const string Input = "bob-picker__input";
            public const string Separator = "bob-picker__separator";
            public const string Slider = "bob-picker__slider";
            public const string Preview = "bob-picker__preview";
        }
    }

    public static class DataAttributes
    {
        // --- Component identification ---
        /// <summary>
        /// Identifies the component type (e.g., "button", "input-text")
        /// </summary>
        public const string Component = "data-bob-component";

        /// <summary>
        /// Component density: compact, standard, comfortable. Affects gap/padding.
        /// </summary>
        public const string Density = "data-bob-density";

        /// <summary>
        /// Component is disabled.
        /// </summary>
        public const string Disabled = "data-bob-disabled";

        /// <summary>
        /// Component is active.
        /// </summary>
        public const string Active = "data-bob-active";

        /// <summary>
        /// Component has validation error.
        /// </summary>
        public const string Error = "data-bob-error";

        /// <summary>
        /// Input label is in floated position.
        /// </summary>
        public const string Floated = "data-bob-floated";

        /// <summary>
        /// Whether component spans full width of container.
        /// </summary>
        public const string FullWidth = "data-bob-fullwidth";

        // --- Component family attributes ---
        /// <summary>
        /// Marks component as part of input family for shared styles.
        /// </summary>
        public const string InputBase = "data-bob-input-base";

        /// <summary>
        /// Marks component as part of picker family for shared styles.
        /// </summary>
        public const string PickerBase = "data-bob-picker-base";

        /// <summary>
        /// Marks component as part of data collection family for shared styles.
        /// </summary>
        public const string DataCollectionBase = "data-bob-data-collection";

        /// <summary>
        /// Picker-family cell rendered with reduced emphasis (e.g. days outside the current month
        /// in a date picker, weekday header cells). Used by the picker family CSS to apply the
        /// muted opacity rule.
        /// </summary>
        public const string Muted = "data-bob-muted";

        // --- State attributes ---
        /// <summary>
        /// Component is in loading state.
        /// </summary>
        public const string Loading = "data-bob-loading";

        /// <summary>
        /// Component is read-only (inputs).
        /// </summary>
        public const string ReadOnly = "data-bob-readonly";

        /// <summary>
        /// Component is required (inputs).
        /// </summary>
        public const string Required = "data-bob-required";

        // --- Behavior attributes ---
        /// <summary>
        /// Whether shadow is applied (activates shadow CSS).
        /// </summary>
        public const string Shadow = "data-bob-shadow";

        /// <summary>
        /// Material Design elevation level (0–24) applied to the component. Set by
        /// <see cref="IHasElevation"/> at render time; the matching surface-tint percentage
        /// is emitted as <see cref="InlineVariables.ElevationTint"/>. Components that opt into
        /// elevation styling select on this attribute under
        /// <c>[data-bob-theme="dark"]</c> to lift their surface.
        /// </summary>
        public const string Elevation = "data-bob-elevation";

        // --- Design attributes ---
        /// <summary>
        /// Active theme id, set on <c>document.documentElement</c> by <c>BOBInitializer</c>
        /// / <c>ThemeInterop</c>. Theme palettes (see <c>BOBThemePaletteBase</c>) emit
        /// <c>html[data-bob-theme="&lt;id&gt;"]</c> blocks that override the default palette
        /// custom properties when this attribute matches.
        /// </summary>
        public const string Theme = "data-bob-theme";

        /// <summary>
        /// Component size: small, medium, large. Sets --bob-size-multiplier.
        /// </summary>
        public const string Size = "data-bob-size";

        /// <summary>
        /// Space-separated list of transition class names.
        /// </summary>
        public const string Transitions = "data-bob-transitions";

        /// <summary>
        /// Set by components while a self-managed transition is mid-flight (e.g. theme switch fade).
        /// CSS reacts via <c>[data-bob-transitioning="true"]</c>; values: "true" | "false".
        /// </summary>
        public const string Transitioning = "data-bob-transitioning";

        /// <summary>
        /// Current variant of the component (e.g., "outlined", "filled")
        /// </summary>
        public const string Variant = "data-bob-variant";

        /// <summary>
        /// Placement of auxiliary buttons within a component (e.g., "left", "right").
        /// </summary>
        public const string ButtonPlacement = "data-bob-button-placement";

        /// <summary>
        /// Resize behavior for resizable elements (e.g., textarea). Values: "none", "vertical",
        /// "horizontal", "both".
        /// </summary>
        public const string Resize = "data-bob-resize";

        /// <summary>
        /// Whether the component auto-grows to fit its content.
        /// </summary>
        public const string AutoResize = "data-bob-autoresize";

        // --- Badge attributes ---
        /// <summary>
        /// Badge rendered as a small dot with no content.
        /// </summary>
        public const string Dot = "data-bob-dot";

        /// <summary>
        /// Badge rendered with a fully circular shape.
        /// </summary>
        public const string Circular = "data-bob-circular";

        /// <summary>
        /// Placement of floating / notification elements (badges, toasts).
        /// </summary>
        public const string Position = "data-bob-position";

        // --- Data collection attributes ---
        /// <summary>
        /// Data-collection rows respond to hover.
        /// </summary>
        public const string Hoverable = "data-bob-hoverable";

        /// <summary>
        /// Name of the row-alternation pattern applied by data-collection components.
        /// </summary>
        public const string RowPattern = "data-bob-row-pattern";

        // --- Form state attributes ---
        /// <summary>
        /// Checkbox / radio mark is currently checked. Tri-state checkboxes use this together
        /// with <see cref="Indeterminate"/> ('checked' wins when both are true).
        /// </summary>
        public const string Checked = "data-bob-checked";

        /// <summary>
        /// Checkbox is in indeterminate (tri-state) mode.
        /// </summary>
        public const string Indeterminate = "data-bob-indeterminate";

        /// <summary>
        /// Single-select vs. multi-select behavior for collections.
        /// </summary>
        public const string SelectionMode = "data-bob-selection-mode";

        /// <summary>
        /// Layout orientation (horizontal / vertical).
        /// </summary>
        public const string Orientation = "data-bob-orientation";

        /// <summary>
        /// Interaction that triggers menu expansion (click / hover).
        /// </summary>
        public const string Trigger = "data-bob-trigger";

        /// <summary>
        /// Whether expanding a menu node collapses siblings.
        /// </summary>
        public const string ExpandMode = "data-bob-expand-mode";

        // --- Toast attributes ---
        /// <summary>
        /// Toast is currently running its close animation.
        /// </summary>
        public const string Closing = "data-bob-closing";

        /// <summary>
        /// Toast auto-close timer is paused (hover).
        /// </summary>
        public const string Paused = "data-bob-paused";

        /// <summary>
        /// Named animation applied to a toast.
        /// </summary>
        public const string Animation = "data-bob-animation";

        // --- Opt-in styling attributes ---
        /// <summary>
        /// Opt-in marker for the BOB global scrollbar styles. Place on <c>&lt;html&gt;</c> (or any ancestor)
        /// to apply branded scrollbars to the consumer app.
        /// </summary>
        public const string Scrollbars = "data-bob-scrollbars";
    }

    public static class InlineVariables
    {
        // --- Color overrides (from IHasColor/IHasBackgroundColor) ---
        public const string BackgroundColor = "--bob-inline-background";
        public const string Color = "--bob-inline-color";

        // --- Border overrides (from IHasBorder) ---
        public const string Border = "--bob-inline-border";

        public const string BorderBottom = "--bob-inline-border-bottom";
        public const string BorderLeft = "--bob-inline-border-left";
        public const string BorderRadius = "--bob-inline-border-radius";
        public const string BorderRight = "--bob-inline-border-right";
        public const string BorderTop = "--bob-inline-border-top";

        // --- Effect overrides (from IHasRipple) ---
        public const string RippleColor = "--bob-inline-ripple-color";

        public const string RippleDuration = "--bob-inline-ripple-duration";

        // --- Shadow variables (from IHasShadow) ---
        public const string Shadow = "--bob-inline-shadow";

        // --- Elevation variables (from IHasElevation) ---
        /// <summary>
        /// Surface-tint percentage emitted as a CSS unit-suffixed value (e.g. <c>"11%"</c>).
        /// Components consume it inside <c>color-mix(...)</c> alongside
        /// <c>var(--palette-surface)</c> and a tint color of their choice (the default in BOB is
        /// <c>var(--palette-surface-contrast)</c>; components free to swap to <c>--palette-primary</c>
        /// or any other token). Resolved by <see cref="BOBElevationPresets.SurfaceTintPercent"/>.
        /// </summary>
        public const string ElevationTint = "--bob-inline-elevation-tint";

        // --- Prefix/Suffix overrides (from IHasPrefix/IHasSuffix) ---
        public const string PrefixColor = "--bob-inline-prefix-color";
        public const string PrefixBackgroundColor = "--bob-inline-prefix-background";
        public const string SuffixColor = "--bob-inline-suffix-color";
        public const string SuffixBackgroundColor = "--bob-inline-suffix-background";

        // --- Switch overrides (BOBSwitch) ---
        public const string SwitchTrackInactiveBackground = "--bob-inline-track-inactive-bg";
        public const string SwitchTrackActiveBackground = "--bob-inline-track-active-bg";
        public const string SwitchThumbInactiveBackground = "--bob-inline-thumb-inactive-bg";
        public const string SwitchThumbActiveBackground = "--bob-inline-thumb-active-bg";
        public const string SwitchThumbInactiveColor = "--bob-inline-thumb-inactive-color";
        public const string SwitchThumbActiveColor = "--bob-inline-thumb-active-color";

        // --- Layout overrides ---
        public const string LayoutSidebarWidth = "--bob-inline-sidebar-width";
        public const string LayoutHeaderHeight = "--bob-inline-header-height";
        public const string LayoutCollapseBreakpoint = "--bob-inline-collapse-breakpoint";
        public const string LayoutContentMaxWidth = "--bob-inline-content-max-width";
        public const string LayoutNavColMin = "--bob-inline-nav-col-min";
        public const string LayoutNavGap = "--bob-inline-nav-gap";
        public const string LayoutNavColumns = "--bob-inline-nav-columns";

        // --- Data collection overrides ---
        public const string DcCardBorder = "--bob-inline-dc-card-border";
        public const string DcCardBorderTop = "--bob-inline-dc-card-border-top";
        public const string DcCardBorderRight = "--bob-inline-dc-card-border-right";
        public const string DcCardBorderBottom = "--bob-inline-dc-card-border-bottom";
        public const string DcCardBorderLeft = "--bob-inline-dc-card-border-left";
        public const string DcCardBorderRadius = "--bob-inline-dc-card-border-radius";
        public const string DcCardShadow = "--bob-inline-dc-card-shadow";
        public const string DcCardBackground = "--bob-inline-dc-card-background";

        public const string DcCellBorder = "--bob-inline-dc-cell-border";
        public const string DcCellBorderTop = "--bob-inline-dc-cell-border-top";
        public const string DcCellBorderRight = "--bob-inline-dc-cell-border-right";
        public const string DcCellBorderBottom = "--bob-inline-dc-cell-border-bottom";
        public const string DcCellBorderLeft = "--bob-inline-dc-cell-border-left";

        public const string DcRowBorder = "--bob-inline-dc-row-border";
        public const string DcRowBorderTop = "--bob-inline-dc-row-border-top";
        public const string DcRowBorderRight = "--bob-inline-dc-row-border-right";
        public const string DcRowBorderBottom = "--bob-inline-dc-row-border-bottom";
        public const string DcRowBorderLeft = "--bob-inline-dc-row-border-left";

        public const string RowPatternBackground = "--bob-inline-row-pattern-bg";
        public const string RowPatternEvenBackground = "--bob-inline-row-pattern-even-bg";
        public const string RowPatternOddBackground = "--bob-inline-row-pattern-odd-bg";
        public const string RowPatternNthBackground = "--bob-inline-row-pattern-nth-bg";
        public const string RowPatternAllBackground = "--bob-inline-row-pattern-all-bg";

        public const string RowPatternBorder = "--bob-inline-row-pattern-border";
        public const string RowPatternBorderTop = "--bob-inline-row-pattern-border-top";
        public const string RowPatternBorderRight = "--bob-inline-row-pattern-border-right";
        public const string RowPatternBorderBottom = "--bob-inline-row-pattern-border-bottom";
        public const string RowPatternBorderLeft = "--bob-inline-row-pattern-border-left";
    }

    public static class Tags
    {
        /// <summary>
        /// Root custom element for all BOB components. Provides isolation and consistent styling anchor.
        /// </summary>
        public const string Component = "bob-component";
    }

    public static class Tokens
    {
        /// <summary>
        /// Opacity values for visual states. Used for: disabled states, placeholders, hover effects.
        /// </summary>
        public static class Opacity
        {
            public const string Disabled = "--bob-opacity-disabled";
            public const string DisabledValue = "0.5";
            public const string Placeholder = "--bob-opacity-placeholder";
            public const string PlaceholderValue = "0.5";
        }

        /// <summary>
        /// Z-index scale for stacking contexts. Used for: overlays, modals, tooltips, dropdowns.
        /// </summary>
        public static class ZIndex
        {
            public const string Dropdown = "--bob-z-dropdown";
            public const string DropdownValue = "1000";
            public const string Modal = "--bob-z-modal";
            public const string ModalValue = "1300";
            public const string Sticky = "--bob-z-sticky";
            public const string StickyValue = "1100";
            public const string Toast = "--bob-z-toast";
            public const string ToastValue = "1500";
            public const string Tooltip = "--bob-z-tooltip";
            public const string TooltipValue = "1400";

        }

        /// <summary>
        /// Size multiplier anchors consumed by [data-bob-size] (see <see cref="ComponentVariables.Size.Multiplier"/>).
        /// </summary>
        public static class Size
        {
            public const string DefaultMultiplierValue = "1";
            public const string SmallMultiplier = "--bob-small-multiplier";
            public const string SmallMultiplierValue = "0.75";
            public const string MediumMultiplier = "--bob-medium-multiplier";
            public const string MediumMultiplierValue = "1";
            public const string LargeMultiplier = "--bob-large-multiplier";
            public const string LargeMultiplierValue = "1.25";
        }

        /// <summary>
        /// Density multiplier anchors consumed by [data-bob-density] (see <see cref="ComponentVariables.Density.Multiplier"/>).
        /// </summary>
        public static class Density
        {
            public const string DefaultMultiplierValue = "1";
            public const string CompactMultiplier = "--bob-compact-multiplier";
            public const string CompactMultiplierValue = "0.75";
            public const string StandardMultiplier = "--bob-standard-multiplier";
            public const string StandardMultiplierValue = "1";
            public const string ComfortableMultiplier = "--bob-comfortable-multiplier";
            public const string ComfortableMultiplierValue = "1.25";
        }

        /// <summary>
        /// Border defaults consumed by components that declare borders via IHasBorder.
        /// </summary>
        public static class Border
        {
            public const string Width = "--bob-border-width";
            public const string WidthValue = "0px";
            public const string Style = "--bob-border-style";
            public const string StyleValue = "solid";
            public const string Radius = "--bob-border-radius";
            public const string RadiusValue = "2px";
        }

        /// <summary>
        /// Focus highlight outline (WCAG 2.4.7). Consumed by <c>:focus-visible</c> in the base focus system.
        /// </summary>
        public static class Highlight
        {
            public const string Outline = "--bob-highlight-outline";
            public const string OutlineValue = "2px solid var(--palette-highlight)";
            public const string OutlineOffset = "--bob-highlight-outline-offset";
            public const string OutlineOffsetValue = "0px";
        }

        /// <summary>
        /// Ripple animation vars and keyframes identifier. Class name lives in <see cref="CssClasses.Ripple"/>.
        /// </summary>
        public static class Ripple
        {
            public const string Color = "--bob-ripple-color";
            public const string ColorFallbackValue = "rgba(255, 255, 255, 0.4)";
            public const string Duration = "--bob-ripple-duration";
            public const string DurationFallbackValue = "600ms";
            public const string Animation = "bob-ripple-animation";
        }

        /// <summary>
        /// Scrollbar dimensions. Styles are opt-in via <see cref="DataAttributes.Scrollbars"/> or the
        /// <see cref="CssClasses.Scrollbars"/> utility class — the library does not touch consumer scrollbars by default.
        /// </summary>
        public static class Scrollbar
        {
            public const string Width = "--bob-scrollbar-width";
            public const string WidthValue = "10px";
            public const string ThumbRadius = "--bob-scrollbar-thumb-radius";
            public const string ThumbRadiusValue = "8px";
            public const string ThumbBorderWidth = "--bob-scrollbar-thumb-border-width";
            public const string ThumbBorderWidthValue = "2px";
        }

        /// <summary>
        /// Input family defaults. Consumed by the input CSS generator and overridable by consumers at <c>:root</c>.
        /// </summary>
        public static class Input
        {
            public const string Radius = "--bob-input-radius";
            public const string RadiusValue = "4px";
            public const string TransitionDuration = "--bob-input-transition-duration";
            public const string TransitionDurationValue = "150ms";
            public const string TransitionEasing = "--bob-input-transition-easing";
            public const string TransitionEasingValue = "cubic-bezier(0.4, 0, 0.2, 1)";
            public const string FloatedScale = "--bob-input-floated-scale";
            public const string FloatedScaleValue = "0.75";
        }

        /// <summary>
        /// Picker family defaults. Consumed by the picker CSS generator and overridable by consumers at <c>:root</c>.
        /// </summary>
        public static class Picker
        {
            public const string Radius = "--bob-picker-radius";
            public const string RadiusValue = "8px";
            public const string CellSize = "--bob-picker-cell-size";
            public const string CellSizeValue = "36px";
            public const string Padding = "--bob-picker-padding";
            public const string PaddingValue = "0.75rem";
        }

        /// <summary>
        /// Transition system (consumed by <see cref="DataAttributes.Transitions"/>).
        /// Variables follow <c>--bob-t-{trigger}-{property}</c>; <see cref="VariableFor"/> builds them.
        /// Vars are intentionally emitted without a default — transitions are opt-in per component and
        /// undefined <c>var()</c> resolves to "unset", so the CSS declaration silently no-ops.
        /// </summary>
        public static class Transitions
        {
            public const string TargetClass = "transition-target";
            public const string Shorthand = "--bob-t-transition";
            public const string VariablePrefix = "--bob-t-";

            public static readonly string[] Triggers =
            [
                "hover",
                "focus",
                "active"
            ];

            public static readonly string[] Props =
            [
                "scale", "rotate", "translate",
                "opacity",
                "filter", "backdrop-filter",
                "box-shadow", "text-shadow",
                "color", "background-color", "border-color", "outline-color",
                "background",
                "border-radius", "outline", "outline-offset",
                "padding", "gap"
            ];

            public static string VariableFor(string trigger, string property) => $"{VariablePrefix}{trigger}-{property}";
        }
    }

    public static class Typography
    {
        public const string FontFamily = "--bob-font-family";
        public const string FontFamilyHeading = "--bob-font-family-heading";
        public const string FontFamilyHeadingValue = "var(--bob-font-family)";
        public const string FontFamilyValue = "system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", Roboto, sans-serif";
        public const string FontMono = "--bob-font-mono";
        public const string FontMonoValue = "ui-monospace, \"Cascadia Mono\", \"SF Mono\", Consolas, monospace";
        public const string FontSizeBase = "--bob-font-size-base";
        /// <summary>Fluid typography: scales from 0.875rem (≈640px viewport) to 1.125rem (≈1536px viewport).</summary>
        public const string FontSizeBaseValue = "clamp(0.875rem, 0.75rem + 0.25vw, 1.125rem)";
        public const string LineHeight = "--bob-line-height";
        public const string LineHeightHeading = "--bob-line-height-heading";
        public const string LineHeightHeadingValue = "1.2";
        public const string LineHeightValue = "1.5";

        /// <summary>Heading scale based on 1.25 ratio (Major Third).</summary>
        public const string H1FontSize = "2.441em";
        public const string H2FontSize = "1.953em";
        public const string H3FontSize = "1.563em";
        public const string H4FontSize = "1.25em";
        public const string H5FontSize = "1em";
        public const string H6FontSize = "0.875em";
        public const string HeadingFontWeight = "700";

        /// <summary>Shared small/secondary text font-size (small, code, kbd, samp, pre).</summary>
        public const string SmallFontSize = "0.875em";
        public const string BoldFontWeight = "700";

        /// <summary>Inline/preformatted code style tokens.</summary>
        public const string CodeFontWeight = "500";
        public const string PreLineHeight = "1.6";

        /// <summary>Anchor/link color transition.</summary>
        public const string LinkTransitionValue = "color 150ms ease";

        /// <summary>Horizontal rule visual weight.</summary>
        public const string HrOpacity = "0.2";
    }

    public static class Values
    {
        public static class Density
        {
            public const string Comfortable = "comfortable";
            public const string Compact = "compact";
            public const string Standard = "standard";
        }

        public static class Size
        {
            public const string Large = "large";
            public const string Medium = "medium";
            public const string Small = "small";
        }

        public static class Variant
        {
            public const string Filled = "filled";
            public const string Outlined = "outlined";
            public const string Standard = "standard";
        }
    }
}