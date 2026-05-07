# Variants

Auto-generated. Each variant is a sealed subclass of
`BlazOrbit.Components.Variant` exposing its built-in instances as static fields
and (when `Custom(string)` is defined) a factory for user-named variants.

## `BOBBadgeVariant`

- **Namespace**: `BlazOrbit.Components`

### Built-in values

- `BOBBadgeVariant.Default`

### Custom factory

```csharp
BOBBadgeVariant.Custom(string name)
```

## `BOBButtonVariant`

- **Namespace**: `BlazOrbit.Components`

### Built-in values

- `BOBButtonVariant.Default`

### Custom factory

```csharp
BOBButtonVariant.Custom(string name)
```

## `BOBCardVariant`

- **Namespace**: `BlazOrbit.Components.Layout`

### Built-in values

- `BOBCardVariant.Default`

### Custom factory

```csharp
BOBCardVariant.Custom(string name)
```

## `BOBInputCheckboxVariant`

- **Namespace**: `BlazOrbit.Components.Forms`

### Built-in values

- `BOBInputCheckboxVariant.Default`

### Custom factory

```csharp
BOBInputCheckboxVariant.Custom(string name)
```

## `BOBInputRadioVariant`

- **Namespace**: `BlazOrbit.Components.Forms`

### Built-in values

- `BOBInputRadioVariant.Default`

### Custom factory

```csharp
BOBInputRadioVariant.Custom(string name)
```

## `BOBInputSwitchVariant`

- **Namespace**: `BlazOrbit.Components.Forms`

### Built-in values

- `BOBInputSwitchVariant.Default`

### Custom factory

```csharp
BOBInputSwitchVariant.Custom(string name)
```

## `BOBInputVariant`

- **Namespace**: `BlazOrbit.Components.Forms`

### Built-in values

- `BOBInputVariant.Filled`
- `BOBInputVariant.Flat`
- `BOBInputVariant.Outlined`
- `BOBInputVariant.Standard`

### Custom factory

```csharp
BOBInputVariant.Custom(string name)
```

## `BOBLoadingIndicatorVariant`

- **Namespace**: `BlazOrbit.Components`

### Built-in values

- `BOBLoadingIndicatorVariant.Bars`
- `BOBLoadingIndicatorVariant.CircularProgress`
- `BOBLoadingIndicatorVariant.Dots`
- `BOBLoadingIndicatorVariant.LinearIndeterminate`
- `BOBLoadingIndicatorVariant.Ring`
- `BOBLoadingIndicatorVariant.Spinner`

### Custom factory

```csharp
BOBLoadingIndicatorVariant.Custom(string name)
```

## `BOBSelectVariant`

- **Namespace**: `BlazOrbit.Components`

### Built-in values

- `BOBSelectVariant.Default`

## `BOBSvgIconVariant`

- **Namespace**: `BlazOrbit.Components`

### Built-in values

- `BOBSvgIconVariant.Default`

### Custom factory

```csharp
BOBSvgIconVariant.Custom(string name)
```

## `BOBTabsVariant`

- **Namespace**: `BlazOrbit.Components`

### Built-in values

- `BOBTabsVariant.Enclosed`
- `BOBTabsVariant.Pills`
- `BOBTabsVariant.Underline`

### Custom factory

```csharp
BOBTabsVariant.Custom(string name)
```

## `BOBThemeSelectorVariant`

- **Namespace**: `BlazOrbit.Components.Layout`

### Built-in values

- `BOBThemeSelectorVariant.Default`
- `BOBThemeSelectorVariant.SunMoon`

### Custom factory

```csharp
BOBThemeSelectorVariant.Custom(string name)
```

## `BOBToastVariant`

- **Namespace**: `BlazOrbit.Components.Layout`

### Built-in values

- `BOBToastVariant.Default`

### Custom factory

```csharp
BOBToastVariant.Custom(string name)
```

## `DataCardsVariant`

- **Namespace**: `BlazOrbit.Components`

### Built-in values

- `DataCardsVariant.Default`

### Custom factory

```csharp
DataCardsVariant.Custom(string name)
```

## `DataGridVariant`

- **Namespace**: `BlazOrbit.Components`

### Built-in values

- `DataGridVariant.Default`

### Custom factory

```csharp
DataGridVariant.Custom(string name)
```

## Registration

```csharp
builder.Services.AddBlazOrbitVariants(b =>
{
    b.ForComponent<BOBButton>()
     .AddVariant(BOBButtonVariant.Custom("MyVariant"), MyTemplates.MyVariant);
});
```

`MyTemplates.MyVariant` is a `RenderFragment<TComponent>` declared inside a
`.razor` file (Razor markup is not legal inside `.cs`). See `patterns.md` and
`recipes.md` for the canonical pattern.
