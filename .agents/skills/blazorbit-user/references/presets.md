<!-- handcrafted: do NOT regenerate. The regenerator only writes
     components.md / variants.md / icons.md. -->

# Enums, Presets, and Value Factories

Reference for the parameter value types used throughout the BlazOrbit component surface.
These are what you pass to component parameters — the actual values, not the wrappers.

> Snapshot generated for the current shipped surface. New presets are additive; renames
> are flagged in the changelog.

## Sizing & density enums

```csharp
namespace BlazOrbit.Components;
public enum BOBSize    { Small, Medium, Large }            // multiplier 0.75 / 1 / 1.25
public enum BOBDensity { Compact, Standard, Comfortable }  // multiplier 0.75 / 1 / 1.25
```

## Colors — `PaletteColor` (BlazOrbit.Core.Css)

Static instances with an implicit `string` conversion to `var(--palette-<name>)`. Pass
anywhere a `string?` color parameter is expected (`Color`, `BackgroundColor`, `RippleColor`, …).

```csharp
PaletteColor.Primary,    PaletteColor.PrimaryContrast
PaletteColor.Secondary,  PaletteColor.SecondaryContrast
PaletteColor.Surface,    PaletteColor.SurfaceContrast
PaletteColor.Background, PaletteColor.BackgroundContrast
PaletteColor.Error,      PaletteColor.ErrorContrast
PaletteColor.Success,    PaletteColor.SuccessContrast
PaletteColor.Warning,    PaletteColor.WarningContrast
PaletteColor.Info,       PaletteColor.InfoContrast
PaletteColor.Border, PaletteColor.Highlight, PaletteColor.Shadow
```

## Borders — `BorderStyle` + `BOBBorderPresets` (BlazOrbit.Core.Abstractions.Behaviors.Design)

Fluent factory:

```csharp
BorderStyle.Create()
    .All("2px", BorderStyleType.Solid, PaletteColor.Primary)
    .Radius(8);

// Per-side:
BorderStyle.Create()
    .Top("1px", BorderStyleType.Solid, PaletteColor.Border)
    .Bottom("3px", BorderStyleType.Solid, PaletteColor.Primary)
    .Radius(4);

public enum BorderStyleType
{
    None, Solid, Dashed, Dotted, Double, Groove, Ridge, Inset, Outset
}
```

Presets (`BOBBorderPresets.<Name>`): `Default`, `None`, `Subtle`, `Strong`, `Rounded`,
`RoundedLarge`, `Pill`, `Dashed`, `Dotted`, `Double`, `Primary`, `Secondary`, `Success`,
`Warning`, `Error`, `Info`.

## Shadows — `ShadowStyle` + `BOBShadowPresets`

```csharp
ShadowStyle.Create(y: 4, blur: 8, opacity: 0.15f, x: 0, spread: 0,
                   color: PaletteColor.Shadow, inset: false)
    .Add(y: 1, blur: 2, opacity: 0.20f);

// Material-style elevation (level 0–24):
BOBShadowPresets.Elevation(2);
BOBShadowPresets.Elevation(8, color: PaletteColor.Primary);
```

## Transitions — `BOBTransitions` + `BOBTransitionPresets`

Pass a preset to any `Transitions` parameter (`BOBButton.Transitions`, etc.):

```csharp
BOBTransitionPresets.HoverScale
BOBTransitionPresets.HoverShadow
BOBTransitionPresets.HoverFade
BOBTransitionPresets.HoverLift
BOBTransitionPresets.HoverGlow
BOBTransitionPresets.CardHover
BOBTransitionPresets.FocusRing
BOBTransitionPresets.Interactive
BOBTransitionPresets.MaterialButton
BOBTransitionPresets.PremiumButton
BOBTransitionPresets.GlassMorphism
BOBTransitionPresets.Neumorphism
```

To compose a custom transition, instantiate `BOBTransitions` and add per-trigger
entries (hover/focus/active/disabled) targeting the property you want animated.

## Icons — `BOBIconKeys` (catalog) + `IconKey` (struct)

`IconKey` is a `record struct` with no public instances — always go through `BOBIconKeys`.

```csharp
// Semantic UI set (curated, ~20–30 icons):
BOBIconKeys.UI.Save, BOBIconKeys.UI.Close, BOBIconKeys.UI.Check,
BOBIconKeys.UI.Menu, BOBIconKeys.UI.Search, BOBIconKeys.UI.Settings, ...

// Full Material Icons (Outlined / Round / Sharp variants):
BOBIconKeys.MaterialIconsOutlined.i_<icon-name>
BOBIconKeys.MaterialIconsRound.i_<icon-name>
BOBIconKeys.MaterialIconsSharp.i_<icon-name>

// Brand & file-format icons:
BOBIconKeys.Brands.<Name>
BOBIconKeys.FileFormats.<Name>
```

## Layout & data enums

```csharp
// Card
public enum CardMediaPosition    { Top, AfterHeader, BeforeActions, Bottom }
public enum CardActionsAlignment { Start, Center, End, SpaceBetween }

// Sidebar layout
public enum SidebarSide { Start, End }

// Accordion
public enum BOBAccordionMode { Single, SingleStrict, Multiple }

// Data collections
public enum ColumnAlign    { Left, Center, Right }
public enum SortDirection  { None, Ascending, Descending }
public enum SelectionMode  { None, Single, Multiple }
public enum FilterMode     { None, Contains, StartsWith, EndsWith, Equals, Custom }
```

## Toast — `ToastOptions`, `ToastSeverity`, `ToastPosition`, `ToastAnimation`

```csharp
public enum ToastSeverity { Info, Success, Warning, Error }
public enum ToastPosition { TopLeft, TopCenter, TopRight, BottomLeft, BottomCenter, BottomRight }

public sealed class ToastOptions
{
    public static ToastOptions Default    { get; } // 5s, top-right, slide+fade
    public static ToastOptions Quick      { get; } // 2s
    public static ToastOptions Long       { get; } // 10s
    public static ToastOptions Persistent { get; } // no auto-dismiss

    public ToastAnimation Animation   { get; init; }
    public bool           AutoDismiss { get; init; } = true;
    public bool           Closable    { get; init; } = true;
    public string?        CssClass    { get; init; }
    public TimeSpan       Duration    { get; init; } = TimeSpan.FromSeconds(5);
    public Action?        OnClick     { get; init; }
    public Action?        OnClose     { get; init; }
    public ToastPosition  Position    { get; init; } = ToastPosition.TopRight;
    public int?           Elevation   { get; init; }
    public ToastSeverity  Severity    { get; init; } = ToastSeverity.Info;
}

public sealed class ToastAnimation
{
    public static ToastAnimation Default      { get; }
    public static ToastAnimation FadeOnly     { get; }
    public static ToastAnimation SlideOnly    { get; }
    public static ToastAnimation SlideAndFade { get; }
    public static ToastAnimation None         { get; }

    public ToastAnimationType Type     { get; init; }
    public TimeSpan           Duration { get; init; } = TimeSpan.FromMilliseconds(300);
    public string             Easing   { get; init; } = "ease-out";

    public ToastAnimation WithDuration(TimeSpan duration);
    public ToastAnimation WithEasing(string easing);
}
```

Tweak with `with`:

```csharp
ToastOptions.Default with { Severity = ToastSeverity.Success, Duration = TimeSpan.FromSeconds(3) }
```

## Modal — `DialogOptions`, `DrawerOptions`

```csharp
public abstract class ModalOptionsBase
{
    public bool    Closable             { get; set; } = true;
    public bool    CloseOnEscape        { get; set; } = true;
    public bool    CloseOnOverlayClick  { get; set; } = true;
    public string? CssClass             { get; set; }
    public int?    Elevation            { get; set; }
}

public sealed class DialogOptions : ModalOptionsBase
{
    public bool    FullScreen { get; set; }
    public string? MaxWidth   { get; set; } = "90vw";
    public string? MaxHeight  { get; set; } = "90vh";
    public string? MinWidth   { get; set; } = "300px";
    public string? MinHeight  { get; set; }
    public string? Title      { get; set; }
}

public sealed class DrawerOptions : ModalOptionsBase
{
    public DrawerPosition Position { get; set; } = DrawerPosition.Right;
    public string         Size     { get; set; } = "300px";
}

public enum DrawerPosition { Left, Right, Top, Bottom }
```

## Variant identifiers

Each variant component exposes a sealed `Variant` subtype with built-in static instances and
a `.Custom(string name)` factory for user-defined variants. See `variants.md` for the full
list and namespaces. Examples:

```csharp
BOBInputVariant.Filled, BOBInputVariant.Outlined, BOBInputVariant.Standard, BOBInputVariant.Flat
BOBTabsVariant.Underline, BOBTabsVariant.Pills, BOBTabsVariant.Enclosed
BOBLoadingIndicatorVariant.Spinner, .CircularProgress, .Ring, .Dots, .Bars, .LinearIndeterminate
BOBThemeSelectorVariant.Default, .SunMoon

// Custom — use a stable, cached identifier for both registration and consumption:
private static readonly BOBButtonVariant Gradient = BOBButtonVariant.Custom("Gradient");
```

