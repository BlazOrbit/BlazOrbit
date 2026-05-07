# Component Catalog

Auto-generated from compiled assemblies. Lists every public component derived
from `Microsoft.AspNetCore.Components.ComponentBase` and **all** of its
`[Parameter]` properties — including those inherited from `BOBComponentBase`,
`BOBVariantComponentBase<,>`, `BOBInputComponentBase<,,>`,
`BOBDataCollectionBase<,,>`, and `Microsoft.AspNetCore.Components.Forms.InputBase<>`.

> Inherited parameters (e.g. `Items`, `Hoverable`, `Disabled`, `ReadOnly`,
> `Required`, `Variant`, `Value`, `ValueChanged`, `ValueExpression`) are listed
> alongside the component's own. The `Declared on` column tells you which
> class introduces each parameter so you can locate the contract.

## `BOBAccordion`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasBorder`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `Border` | `BorderStyle` | `BOBAccordion` |
| `ChildContent` | `RenderFragment` | `BOBAccordion` |
| `ExpandedItems` | `IReadOnlyList<string>` | `BOBAccordion` |
| `ExpandedItemsChanged` | `EventCallback<IReadOnlyList<string>>` | `BOBAccordion` |
| `Gap` | `string` | `BOBAccordion` |
| `Mode` | `BOBAccordionMode` | `BOBAccordion` |
| `OnExpandedChanged` | `EventCallback<BOBAccordionItemToggle>` | `BOBAccordion` |
| `Separator` | `RenderFragment` | `BOBAccordion` |
| `Size` | `BOBSize` | `BOBAccordion` |

## `BOBAccordionItem`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ChildContent` | `RenderFragment` | `BOBAccordionItem` |
| `Disabled` | `bool` | `BOBAccordionItem` |
| `ExpandIcon` | `IconKey?` | `BOBAccordionItem` |
| `ExpandIconColor` | `string` | `BOBAccordionItem` |
| `Header` | `RenderFragment` | `BOBAccordionItem` |
| `Id` | `string` | `BOBAccordionItem` |
| `InitiallyExpanded` | `bool` | `BOBAccordionItem` |

## `BOBBadge`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBVariantComponentBase<BOBBadge, BOBBadgeVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasBorder`, `IHasColor`, `IHasShadow`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `BackgroundColor` | `string` | `BOBBadge` |
| `Border` | `BorderStyle` | `BOBBadge` |
| `ChildContent` | `RenderFragment` | `BOBBadge` |
| `Circular` | `bool` | `BOBBadge` |
| `Color` | `string` | `BOBBadge` |
| `Shadow` | `ShadowStyle` | `BOBBadge` |
| `Size` | `BOBSize` | `BOBBadge` |
| `Variant` | `BOBBadgeVariant` | `BOBVariantComponentBase` |

## `BOBBlazorLayout`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `LayoutComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `Body` | `RenderFragment` | `LayoutComponentBase` |

## `BOBButton`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBVariantComponentBase<BOBButton, BOBButtonVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasBorder`, `IHasColor`, `IHasDensity`, `IHasDisabled`, `IHasFullWidth`, `IHasLoading`, `IHasRipple`, `IHasShadow`, `IHasSize`, `IHasTransitions`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `AriaLabel` | `string` | `BOBButton` |
| `BackgroundColor` | `string` | `BOBButton` |
| `Border` | `BorderStyle` | `BOBButton` |
| `ChildContent` | `RenderFragment` | `BOBButton` |
| `Color` | `string` | `BOBButton` |
| `Density` | `BOBDensity` | `BOBButton` |
| `Disabled` | `bool` | `BOBButton` |
| `DisableRipple` | `bool` | `BOBButton` |
| `FullWidth` | `bool` | `BOBButton` |
| `LeadingIcon` | `IconKey?` | `BOBButton` |
| `Loading` | `bool` | `BOBButton` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBButton` |
| `OnClick` | `EventCallback<MouseEventArgs>` | `BOBButton` |
| `RippleColor` | `string` | `BOBButton` |
| `RippleDurationMs` | `int?` | `BOBButton` |
| `Shadow` | `ShadowStyle` | `BOBButton` |
| `Size` | `BOBSize` | `BOBButton` |
| `Text` | `string` | `BOBButton` |
| `TrailingIcon` | `IconKey?` | `BOBButton` |
| `Transitions` | `BOBTransitions` | `BOBButton` |
| `Variant` | `BOBButtonVariant` | `BOBVariantComponentBase` |

## `BOBCard`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBVariantComponentBase<BOBCard, BOBCardVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasBorder`, `IHasColor`, `IHasElevation`, `IHasShadow`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `Actions` | `RenderFragment` | `BOBCard` |
| `ActionsAlignment` | `CardActionsAlignment` | `BOBCard` |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `BackgroundColor` | `string` | `BOBCard` |
| `Border` | `BorderStyle` | `BOBCard` |
| `ChildContent` | `RenderFragment` | `BOBCard` |
| `Clickable` | `bool` | `BOBCard` |
| `Color` | `string` | `BOBCard` |
| `Elevation` | `int?` | `BOBCard` |
| `Header` | `RenderFragment` | `BOBCard` |
| `Media` | `RenderFragment` | `BOBCard` |
| `MediaHeight` | `string` | `BOBCard` |
| `MediaPosition` | `CardMediaPosition` | `BOBCard` |
| `OnClick` | `EventCallback<MouseEventArgs>` | `BOBCard` |
| `Shadow` | `ShadowStyle` | `BOBCard` |
| `Variant` | `BOBCardVariant` | `BOBVariantComponentBase` |

## `BOBCarousel`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ActiveIndex` | `int` | `BOBCarousel` |
| `ActiveIndexChanged` | `EventCallback<int>` | `BOBCarousel` |
| `ActiveIndicatorIcon` | `IconKey?` | `BOBCarousel` |
| `ActiveIndicatorIconColor` | `string` | `BOBCarousel` |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `ArrowIconLeft` | `IconKey?` | `BOBCarousel` |
| `ArrowIconRight` | `IconKey?` | `BOBCarousel` |
| `AutoPlay` | `bool` | `BOBCarousel` |
| `AutoPlayInterval` | `TimeSpan` | `BOBCarousel` |
| `ChildContent` | `RenderFragment` | `BOBCarousel` |
| `IndicatorIcon` | `IconKey?` | `BOBCarousel` |
| `IndicatorIconColor` | `string` | `BOBCarousel` |
| `Loop` | `bool` | `BOBCarousel` |
| `NextLabel` | `string` | `BOBCarousel` |
| `PreviousLabel` | `string` | `BOBCarousel` |
| `ShowArrows` | `bool` | `BOBCarousel` |
| `ShowIndicators` | `bool` | `BOBCarousel` |
| `Size` | `BOBSize` | `BOBCarousel` |
| `SwipeThresholdPx` | `int` | `BOBCarousel` |
| `Transition` | `BOBCarouselTransition` | `BOBCarousel` |
| `TransitionDurationMs` | `int` | `BOBCarousel` |
| `WheelRadiusPx` | `int` | `BOBCarousel` |

## `BOBCarouselItem`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AriaLabel` | `string` | `BOBCarouselItem` |
| `ChildContent` | `RenderFragment` | `BOBCarouselItem` |

## `BOBCodeBlock`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasBackgroundColor`, `IHasBorder`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `BackgroundColor` | `string` | `BOBCodeBlock` |
| `Border` | `BorderStyle` | `BOBCodeBlock` |
| `Code` | `string` | `BOBCodeBlock` |
| `Language` | `SyntaxHighlightLanguage` | `BOBCodeBlock` |
| `Size` | `BOBSize` | `BOBCodeBlock` |
| `Title` | `string` | `BOBCodeBlock` |

## `BOBColorPicker`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBComponentBase`
- **Implements**: `IPickerFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `OnRevert` | `EventCallback` | `BOBColorPicker` |
| `OutputFormat` | `ColorOutputFormats` | `BOBColorPicker` |
| `RevertText` | `string` | `BOBColorPicker` |
| `ShowActions` | `bool` | `BOBColorPicker` |
| `Value` | `CssColor` | `BOBColorPicker` |
| `ValueChanged` | `EventCallback<CssColor>` | `BOBColorPicker` |

## `BOBDataCards<TItem>`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBDataCollectionBase<TItem, BOBDataCards<TItem>, DataCardsVariant>`
- **Implements**: `IDataCollectionFamilyComponent`, `IHasBackgroundColor`, `IHasBorder`, `IHasDensity`, `IHasShadow`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `BackgroundColor` | `string` | `BOBDataCollectionBase` |
| `Border` | `BorderStyle` | `BOBDataCollectionBase` |
| `CardBackground` | `string` | `BOBDataCards` |
| `CardBorder` | `BorderStyle` | `BOBDataCards` |
| `CardFooterTemplate` | `RenderFragment<TItem>` | `BOBDataCards` |
| `CardHeaderTemplate` | `RenderFragment<TItem>` | `BOBDataCards` |
| `CardShadow` | `ShadowStyle` | `BOBDataCards` |
| `CardTemplate` | `RenderFragment<TItem>` | `BOBDataCards` |
| `Columns` | `RenderFragment` | `BOBDataCollectionBase` |
| `ColumnsCount` | `int` | `BOBDataCards` |
| `CustomFilter` | `Func<TItem, string, bool>` | `BOBDataCollectionBase` |
| `DefaultSortColumn` | `string` | `BOBDataCollectionBase` |
| `DefaultSortDirection` | `SortDirection?` | `BOBDataCollectionBase` |
| `Density` | `BOBDensity` | `BOBDataCollectionBase` |
| `EmptyContent` | `RenderFragment` | `BOBDataCollectionBase` |
| `EnableVirtualization` | `bool` | `BOBDataCollectionBase` |
| `Filterable` | `bool` | `BOBDataCollectionBase` |
| `FilterPlaceholder` | `string` | `BOBDataCollectionBase` |
| `Gap` | `string` | `BOBDataCards` |
| `Height` | `string` | `BOBDataCollectionBase` |
| `Hoverable` | `bool` | `BOBDataCollectionBase` |
| `ItemPattern` | `RowStylePattern` | `BOBDataCollectionBase` |
| `Items` | `IEnumerable<TItem>` | `BOBDataCollectionBase` |
| `Loading` | `bool` | `BOBDataCollectionBase` |
| `LoadingContent` | `RenderFragment` | `BOBDataCollectionBase` |
| `MinCardWidth` | `string` | `BOBDataCards` |
| `OnFilter` | `EventCallback<DataCollectionFilterEventArgs>` | `BOBDataCollectionBase` |
| `OnPageChange` | `EventCallback<DataCollectionPageChangeEventArgs>` | `BOBDataCollectionBase` |
| `OnRowClick` | `EventCallback<TItem>` | `BOBDataCollectionBase` |
| `OnSort` | `EventCallback<DataCollectionSortEventArgs>` | `BOBDataCollectionBase` |
| `PageSize` | `int?` | `BOBDataCollectionBase` |
| `PageSizeOptions` | `int[]` | `BOBDataCollectionBase` |
| `SelectedItems` | `HashSet<TItem>` | `BOBDataCollectionBase` |
| `SelectedItemsChanged` | `EventCallback<HashSet<TItem>>` | `BOBDataCollectionBase` |
| `SelectionMode` | `SelectionMode` | `BOBDataCollectionBase` |
| `Shadow` | `ShadowStyle` | `BOBDataCollectionBase` |
| `ShowPageSizeSelector` | `bool` | `BOBDataCollectionBase` |
| `Size` | `BOBSize` | `BOBDataCollectionBase` |
| `Sortable` | `bool` | `BOBDataCollectionBase` |
| `Variant` | `DataCardsVariant` | `BOBVariantComponentBase` |

## `BOBDataColumn<TItem>`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `Align` | `ColumnAlign` | `BOBDataColumn` |
| `CellClass` | `string` | `BOBDataColumn` |
| `CustomComparer` | `Func<TItem, TItem, int>` | `BOBDataColumn` |
| `CustomFilter` | `Func<TItem, string, bool>` | `BOBDataColumn` |
| `Filterable` | `bool` | `BOBDataColumn` |
| `Format` | `string` | `BOBDataColumn` |
| `Header` | `string` | `BOBDataColumn` |
| `HeaderClass` | `string` | `BOBDataColumn` |
| `HeaderTemplate` | `RenderFragment` | `BOBDataColumn` |
| `Property` | `Expression<Func<TItem, object>>` | `BOBDataColumn` |
| `Sortable` | `bool` | `BOBDataColumn` |
| `Template` | `RenderFragment<TItem>` | `BOBDataColumn` |
| `Visible` | `bool` | `BOBDataColumn` |
| `Width` | `string` | `BOBDataColumn` |

## `BOBDataGrid<TItem>`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBDataCollectionBase<TItem, BOBDataGrid<TItem>, DataGridVariant>`
- **Implements**: `IDataCollectionFamilyComponent`, `IHasBackgroundColor`, `IHasBorder`, `IHasDensity`, `IHasShadow`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `BackgroundColor` | `string` | `BOBDataCollectionBase` |
| `Border` | `BorderStyle` | `BOBDataCollectionBase` |
| `CellBorder` | `BorderStyle` | `BOBDataGrid` |
| `Columns` | `RenderFragment` | `BOBDataCollectionBase` |
| `CustomFilter` | `Func<TItem, string, bool>` | `BOBDataCollectionBase` |
| `DefaultSortColumn` | `string` | `BOBDataCollectionBase` |
| `DefaultSortDirection` | `SortDirection?` | `BOBDataCollectionBase` |
| `Density` | `BOBDensity` | `BOBDataCollectionBase` |
| `EmptyContent` | `RenderFragment` | `BOBDataCollectionBase` |
| `EnableVirtualization` | `bool` | `BOBDataCollectionBase` |
| `Filterable` | `bool` | `BOBDataCollectionBase` |
| `FilterPlaceholder` | `string` | `BOBDataCollectionBase` |
| `FixedHeader` | `bool` | `BOBDataGrid` |
| `FooterTemplate` | `RenderFragment` | `BOBDataGrid` |
| `Height` | `string` | `BOBDataCollectionBase` |
| `Hoverable` | `bool` | `BOBDataCollectionBase` |
| `ItemPattern` | `RowStylePattern` | `BOBDataCollectionBase` |
| `Items` | `IEnumerable<TItem>` | `BOBDataCollectionBase` |
| `Loading` | `bool` | `BOBDataCollectionBase` |
| `LoadingContent` | `RenderFragment` | `BOBDataCollectionBase` |
| `OnFilter` | `EventCallback<DataCollectionFilterEventArgs>` | `BOBDataCollectionBase` |
| `OnPageChange` | `EventCallback<DataCollectionPageChangeEventArgs>` | `BOBDataCollectionBase` |
| `OnRowClick` | `EventCallback<TItem>` | `BOBDataCollectionBase` |
| `OnSort` | `EventCallback<DataCollectionSortEventArgs>` | `BOBDataCollectionBase` |
| `PageSize` | `int?` | `BOBDataCollectionBase` |
| `PageSizeOptions` | `int[]` | `BOBDataCollectionBase` |
| `RowBorder` | `BorderStyle` | `BOBDataGrid` |
| `RowTemplate` | `RenderFragment<TItem>` | `BOBDataGrid` |
| `SelectedItems` | `HashSet<TItem>` | `BOBDataCollectionBase` |
| `SelectedItemsChanged` | `EventCallback<HashSet<TItem>>` | `BOBDataCollectionBase` |
| `SelectionMode` | `SelectionMode` | `BOBDataCollectionBase` |
| `Shadow` | `ShadowStyle` | `BOBDataCollectionBase` |
| `ShowPageSizeSelector` | `bool` | `BOBDataCollectionBase` |
| `Size` | `BOBSize` | `BOBDataCollectionBase` |
| `Sortable` | `bool` | `BOBDataCollectionBase` |
| `Variant` | `DataGridVariant` | `BOBVariantComponentBase` |

## `BOBDatePicker`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasDensity`, `IHasSize`, `IPickerFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `Density` | `BOBDensity` | `BOBDatePicker` |
| `Size` | `BOBSize` | `BOBDatePicker` |
| `Value` | `DateOnly?` | `BOBDatePicker` |
| `ValueChanged` | `EventCallback<DateOnly?>` | `BOBDatePicker` |

## `BOBDateTimePattern`

- **Namespace**: `BlazOrbit.Components.Utils`
- **Base**: `BOBBasePattern`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ContainerClass` | `string` | `BOBDateTimePattern` |
| `Disabled` | `bool` | `BOBDateTimePattern` |
| `Editable` | `bool` | `BOBBasePattern` |
| `Format` | `string` | `BOBBasePattern` |
| `Id` | `string` | `BOBDateTimePattern` |
| `OnBlur` | `EventCallback<FocusEventArgs>` | `BOBDateTimePattern` |
| `OnDirtyStateChanged` | `EventCallback<bool>` | `BOBBasePattern` |
| `OnFocus` | `EventCallback<FocusEventArgs>` | `BOBDateTimePattern` |
| `ShowPattern` | `bool` | `BOBDateTimePattern` |
| `Text` | `string` | `BOBBasePattern` |
| `TextChanged` | `EventCallback<string>` | `BOBBasePattern` |

## `BOBDialog`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasElevation`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `Closable` | `bool` | `BOBDialog` |
| `CloseOnEscape` | `bool` | `BOBDialog` |
| `CloseOnOverlayClick` | `bool` | `BOBDialog` |
| `Content` | `RenderFragment` | `BOBDialog` |
| `Elevation` | `int?` | `BOBDialog` |
| `Footer` | `RenderFragment` | `BOBDialog` |
| `FullScreen` | `bool` | `BOBDialog` |
| `Header` | `RenderFragment` | `BOBDialog` |
| `MaxHeight` | `string` | `BOBDialog` |
| `MaxWidth` | `string` | `BOBDialog` |
| `MinHeight` | `string` | `BOBDialog` |
| `MinWidth` | `string` | `BOBDialog` |
| `Open` | `bool` | `BOBDialog` |
| `OpenChanged` | `EventCallback<bool>` | `BOBDialog` |
| `Title` | `string` | `BOBDialog` |

## `BOBDraggable`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `ChildContent` | `RenderFragment` | `BOBDraggable` |
| `Disabled` | `bool` | `BOBDraggable` |
| `OnDrag` | `EventCallback<DragEventArgs>` | `BOBDraggable` |
| `OnDragEnd` | `EventCallback<DragEventArgs>` | `BOBDraggable` |
| `OnDragStart` | `EventCallback<DragEventArgs>` | `BOBDraggable` |
| `PreventDefault` | `bool` | `BOBDraggable` |

## `BOBDrawer`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasElevation`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `ChildContent` | `RenderFragment` | `BOBDrawer` |
| `Closable` | `bool` | `BOBDrawer` |
| `CloseOnEscape` | `bool` | `BOBDrawer` |
| `CloseOnOverlayClick` | `bool` | `BOBDrawer` |
| `Elevation` | `int?` | `BOBDrawer` |
| `Footer` | `RenderFragment` | `BOBDrawer` |
| `Header` | `RenderFragment` | `BOBDrawer` |
| `Open` | `bool` | `BOBDrawer` |
| `OpenChanged` | `EventCallback<bool>` | `BOBDrawer` |
| `Position` | `DrawerPosition` | `BOBDrawer` |
| `Size` | `string` | `BOBDrawer` |

## `BOBDropdownContainer<TValue>`

- **Namespace**: `BlazOrbit.Components.Forms.Dropdown`
- **Base**: `BOBInputComponentBase<TValue, BOBDropdownContainer<TValue>, BOBInputVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasDisabled`, `IHasElevation`, `IHasError`, `IHasLoading`, `IHasPrefix`, `IHasReadOnly`, `IHasRequired`, `IHasShadow`, `IHasSize`, `IHasSuffix`, `IInputFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `AriaMultiselectable` | `bool` | `BOBDropdownContainer` |
| `AriaRole` | `string` | `BOBDropdownContainer` |
| `BackgroundColor` | `string` | `BOBDropdownContainer` |
| `Color` | `string` | `BOBDropdownContainer` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `DisplayValue` | `string` | `BOBDropdownContainer` |
| `Elevation` | `int?` | `BOBDropdownContainer` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `HasValue` | `bool` | `BOBDropdownContainer` |
| `HelperText` | `string` | `BOBDropdownContainer` |
| `Label` | `string` | `BOBDropdownContainer` |
| `Loading` | `bool` | `BOBDropdownContainer` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBDropdownContainer` |
| `MenuContent` | `RenderFragment` | `BOBDropdownContainer` |
| `OnClosed` | `EventCallback` | `BOBDropdownContainer` |
| `OnKeyboardNavigation` | `EventCallback<DropdownKeyboardEventArgs>` | `BOBDropdownContainer` |
| `OnOpened` | `EventCallback` | `BOBDropdownContainer` |
| `OnValueChanged` | `EventCallback<TValue>` | `BOBDropdownContainer` |
| `Placeholder` | `string` | `BOBDropdownContainer` |
| `Placement` | `DropdownPlacement` | `BOBDropdownContainer` |
| `PrefixBackgroundColor` | `string` | `BOBDropdownContainer` |
| `PrefixColor` | `string` | `BOBDropdownContainer` |
| `PrefixIcon` | `IconKey?` | `BOBDropdownContainer` |
| `PrefixText` | `string` | `BOBDropdownContainer` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Shadow` | `ShadowStyle` | `BOBDropdownContainer` |
| `Size` | `BOBSize` | `BOBDropdownContainer` |
| `SuffixBackgroundColor` | `string` | `BOBDropdownContainer` |
| `SuffixColor` | `string` | `BOBDropdownContainer` |
| `SuffixIcon` | `IconKey?` | `BOBDropdownContainer` |
| `SuffixText` | `string` | `BOBDropdownContainer` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `Value` | `TValue` | `InputBase` |
| `ValueChanged` | `EventCallback<TValue>` | `InputBase` |
| `ValueExpression` | `Expression<Func<TValue>>` | `InputBase` |
| `Variant` | `BOBInputVariant` | `BOBInputComponentBase` |

## `BOBFlexStack`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasBackgroundColor`, `IHasBorder`, `IHasColor`, `IHasFullWidth`, `IHasShadow`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `AlignContent` | `FlexStackAlignContent` | `BOBFlexStack` |
| `AlignItems` | `FlexStackAlignItems` | `BOBFlexStack` |
| `BackgroundColor` | `string` | `BOBFlexStack` |
| `Border` | `BorderStyle` | `BOBFlexStack` |
| `ChildContent` | `RenderFragment` | `BOBFlexStack` |
| `Color` | `string` | `BOBFlexStack` |
| `ColumnGap` | `string` | `BOBFlexStack` |
| `Direction` | `FlexStackDirection` | `BOBFlexStack` |
| `FullWidth` | `bool` | `BOBFlexStack` |
| `Gap` | `string` | `BOBFlexStack` |
| `JustifyContent` | `FlexStackJustifyContent` | `BOBFlexStack` |
| `M` | `string` | `BOBFlexStack` |
| `Mb` | `string` | `BOBFlexStack` |
| `Ml` | `string` | `BOBFlexStack` |
| `Mr` | `string` | `BOBFlexStack` |
| `Mt` | `string` | `BOBFlexStack` |
| `Mx` | `string` | `BOBFlexStack` |
| `My` | `string` | `BOBFlexStack` |
| `P` | `string` | `BOBFlexStack` |
| `Pb` | `string` | `BOBFlexStack` |
| `Pl` | `string` | `BOBFlexStack` |
| `Pr` | `string` | `BOBFlexStack` |
| `Pt` | `string` | `BOBFlexStack` |
| `Px` | `string` | `BOBFlexStack` |
| `Py` | `string` | `BOBFlexStack` |
| `RowGap` | `string` | `BOBFlexStack` |
| `Shadow` | `ShadowStyle` | `BOBFlexStack` |
| `Wrap` | `FlexStackWrap` | `BOBFlexStack` |

## `BOBGrid`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `AlignItems` | `GridAlignItems` | `BOBGrid` |
| `ChildContent` | `RenderFragment` | `BOBGrid` |
| `ColumnGap` | `string` | `BOBGrid` |
| `Columns` | `int?` | `BOBGrid` |
| `Direction` | `GridDirection` | `BOBGrid` |
| `DirectionLg` | `GridDirection?` | `BOBGrid` |
| `DirectionMd` | `GridDirection?` | `BOBGrid` |
| `DirectionSm` | `GridDirection?` | `BOBGrid` |
| `DirectionXl` | `GridDirection?` | `BOBGrid` |
| `DirectionXs` | `GridDirection?` | `BOBGrid` |
| `Gap` | `string` | `BOBGrid` |
| `GapLg` | `string` | `BOBGrid` |
| `GapMd` | `string` | `BOBGrid` |
| `GapSm` | `string` | `BOBGrid` |
| `GapXl` | `string` | `BOBGrid` |
| `GapXs` | `string` | `BOBGrid` |
| `JustifyContent` | `GridJustifyContent` | `BOBGrid` |
| `M` | `string` | `BOBGrid` |
| `MaxWidth` | `string` | `BOBGrid` |
| `Mb` | `string` | `BOBGrid` |
| `Ml` | `string` | `BOBGrid` |
| `Mr` | `string` | `BOBGrid` |
| `Mt` | `string` | `BOBGrid` |
| `Mx` | `string` | `BOBGrid` |
| `My` | `string` | `BOBGrid` |
| `P` | `string` | `BOBGrid` |
| `Pb` | `string` | `BOBGrid` |
| `Pl` | `string` | `BOBGrid` |
| `Pr` | `string` | `BOBGrid` |
| `Pt` | `string` | `BOBGrid` |
| `Px` | `string` | `BOBGrid` |
| `Py` | `string` | `BOBGrid` |
| `RowGap` | `string` | `BOBGrid` |
| `Wrap` | `GridWrap` | `BOBGrid` |

## `BOBGridItem`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `AlignSelf` | `GridAlignSelf?` | `BOBGridItem` |
| `Auto` | `bool` | `BOBGridItem` |
| `ChildContent` | `RenderFragment` | `BOBGridItem` |
| `HideLg` | `bool` | `BOBGridItem` |
| `HideMd` | `bool` | `BOBGridItem` |
| `HideSm` | `bool` | `BOBGridItem` |
| `HideXl` | `bool` | `BOBGridItem` |
| `HideXs` | `bool` | `BOBGridItem` |
| `Lg` | `int?` | `BOBGridItem` |
| `M` | `string` | `BOBGridItem` |
| `Mb` | `string` | `BOBGridItem` |
| `Md` | `int?` | `BOBGridItem` |
| `Ml` | `string` | `BOBGridItem` |
| `Mr` | `string` | `BOBGridItem` |
| `Mt` | `string` | `BOBGridItem` |
| `Mx` | `string` | `BOBGridItem` |
| `My` | `string` | `BOBGridItem` |
| `Offset` | `int?` | `BOBGridItem` |
| `OffsetLg` | `int?` | `BOBGridItem` |
| `OffsetMd` | `int?` | `BOBGridItem` |
| `OffsetSm` | `int?` | `BOBGridItem` |
| `OffsetXl` | `int?` | `BOBGridItem` |
| `OffsetXs` | `int?` | `BOBGridItem` |
| `Order` | `int?` | `BOBGridItem` |
| `OrderLg` | `int?` | `BOBGridItem` |
| `OrderMd` | `int?` | `BOBGridItem` |
| `OrderSm` | `int?` | `BOBGridItem` |
| `OrderXl` | `int?` | `BOBGridItem` |
| `OrderXs` | `int?` | `BOBGridItem` |
| `P` | `string` | `BOBGridItem` |
| `Pb` | `string` | `BOBGridItem` |
| `Pl` | `string` | `BOBGridItem` |
| `Pr` | `string` | `BOBGridItem` |
| `Pt` | `string` | `BOBGridItem` |
| `Px` | `string` | `BOBGridItem` |
| `Py` | `string` | `BOBGridItem` |
| `ShowLg` | `bool` | `BOBGridItem` |
| `ShowMd` | `bool` | `BOBGridItem` |
| `ShowSm` | `bool` | `BOBGridItem` |
| `ShowXl` | `bool` | `BOBGridItem` |
| `ShowXs` | `bool` | `BOBGridItem` |
| `Sm` | `int?` | `BOBGridItem` |
| `Span` | `int?` | `BOBGridItem` |
| `Xl` | `int?` | `BOBGridItem` |
| `Xs` | `int?` | `BOBGridItem` |

## `BOBInitializer`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ChildContent` | `RenderFragment` | `BOBInitializer` |
| `DefaultTheme` | `string` | `BOBInitializer` |

## `BOBInputCheckbox<TValue>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<TValue, BOBInputCheckbox<TValue>, BOBInputCheckboxVariant>`
- **Implements**: `IHasActive`, `IHasColor`, `IHasDensity`, `IHasDisabled`, `IHasError`, `IHasReadOnly`, `IHasRequired`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `Active` | `bool` | `BOBInputCheckbox` |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `CheckedIcon` | `IconKey?` | `BOBInputCheckbox` |
| `Color` | `string` | `BOBInputCheckbox` |
| `Density` | `BOBDensity` | `BOBInputCheckbox` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `HelperText` | `string` | `BOBInputCheckbox` |
| `IndeterminateIcon` | `IconKey?` | `BOBInputCheckbox` |
| `Label` | `string` | `BOBInputCheckbox` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Size` | `BOBSize` | `BOBInputCheckbox` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `UncheckedIcon` | `IconKey?` | `BOBInputCheckbox` |
| `Value` | `TValue` | `InputBase` |
| `ValueChanged` | `EventCallback<TValue>` | `InputBase` |
| `ValueExpression` | `Expression<Func<TValue>>` | `InputBase` |
| `Variant` | `BOBInputCheckboxVariant` | `BOBInputComponentBase` |

## `BOBInputColor`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<CssColor, BOBInputColor, BOBInputVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasDisabled`, `IHasError`, `IHasLoading`, `IHasPrefix`, `IHasReadOnly`, `IHasRequired`, `IHasShadow`, `IHasSize`, `IHasSuffix`, `IInputFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `BackgroundColor` | `string` | `BOBInputColor` |
| `Clearable` | `bool` | `BOBInputColor` |
| `Color` | `string` | `BOBInputColor` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayMode` | `ColorPickerDisplayMode` | `BOBInputColor` |
| `DisplayName` | `string` | `InputBase` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `HelperText` | `string` | `BOBInputColor` |
| `Label` | `string` | `BOBInputColor` |
| `Loading` | `bool` | `BOBInputColor` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBInputColor` |
| `OutputFormat` | `ColorOutputFormats` | `BOBInputColor` |
| `Placeholder` | `string` | `BOBInputColor` |
| `PrefixBackgroundColor` | `string` | `BOBInputColor` |
| `PrefixColor` | `string` | `BOBInputColor` |
| `PrefixIcon` | `IconKey?` | `BOBInputColor` |
| `PrefixText` | `string` | `BOBInputColor` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `RevertText` | `string` | `BOBInputColor` |
| `Shadow` | `ShadowStyle` | `BOBInputColor` |
| `ShowCopyButton` | `bool` | `BOBInputColor` |
| `Size` | `BOBSize` | `BOBInputColor` |
| `SuffixBackgroundColor` | `string` | `BOBInputColor` |
| `SuffixColor` | `string` | `BOBInputColor` |
| `SuffixIcon` | `IconKey?` | `BOBInputColor` |
| `SuffixText` | `string` | `BOBInputColor` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `Value` | `CssColor` | `InputBase` |
| `ValueChanged` | `EventCallback<CssColor>` | `InputBase` |
| `ValueExpression` | `Expression<Func<CssColor>>` | `InputBase` |
| `Variant` | `BOBInputVariant` | `BOBInputComponentBase` |

## `BOBInputDateTime<TValue>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<TValue, BOBInputDateTime<TValue>, BOBInputVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasDensity`, `IHasDisabled`, `IHasError`, `IHasLoading`, `IHasPrefix`, `IHasReadOnly`, `IHasRequired`, `IHasShadow`, `IHasSize`, `IHasSuffix`, `IInputFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `BackgroundColor` | `string` | `BOBInputDateTime` |
| `Color` | `string` | `BOBInputDateTime` |
| `Density` | `BOBDensity` | `BOBInputDateTime` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `HelperText` | `string` | `BOBInputDateTime` |
| `Label` | `string` | `BOBInputDateTime` |
| `Loading` | `bool` | `BOBInputDateTime` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBInputDateTime` |
| `PrefixBackgroundColor` | `string` | `BOBInputDateTime` |
| `PrefixColor` | `string` | `BOBInputDateTime` |
| `PrefixIcon` | `IconKey?` | `BOBInputDateTime` |
| `PrefixText` | `string` | `BOBInputDateTime` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Shadow` | `ShadowStyle` | `BOBInputDateTime` |
| `Size` | `BOBSize` | `BOBInputDateTime` |
| `SuffixBackgroundColor` | `string` | `BOBInputDateTime` |
| `SuffixColor` | `string` | `BOBInputDateTime` |
| `SuffixIcon` | `IconKey?` | `BOBInputDateTime` |
| `SuffixText` | `string` | `BOBInputDateTime` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `Value` | `TValue` | `InputBase` |
| `ValueChanged` | `EventCallback<TValue>` | `InputBase` |
| `ValueExpression` | `Expression<Func<TValue>>` | `InputBase` |
| `Variant` | `BOBInputVariant` | `BOBInputComponentBase` |

## `BOBInputDropdown<TValue>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `BackgroundColor` | `string` | `BOBInputDropdown` |
| `ChildContent` | `RenderFragment` | `BOBInputDropdown` |
| `CloseOnSelect` | `bool` | `BOBInputDropdown` |
| `Color` | `string` | `BOBInputDropdown` |
| `Density` | `BOBDensity` | `BOBInputDropdown` |
| `DeselectAllText` | `string` | `BOBInputDropdown` |
| `Disabled` | `bool` | `BOBInputDropdown` |
| `FullWidth` | `bool` | `BOBInputDropdown` |
| `HelperText` | `string` | `BOBInputDropdown` |
| `IsLoading` | `bool` | `BOBInputDropdown` |
| `Label` | `string` | `BOBInputDropdown` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBInputDropdown` |
| `NoOptionsTemplate` | `RenderFragment` | `BOBInputDropdown` |
| `NoResultsTemplate` | `RenderFragment<NoResultsContext>` | `BOBInputDropdown` |
| `Placeholder` | `string` | `BOBInputDropdown` |
| `Placement` | `DropdownPlacement` | `BOBInputDropdown` |
| `ReadOnly` | `bool` | `BOBInputDropdown` |
| `Required` | `bool` | `BOBInputDropdown` |
| `Searchable` | `bool` | `BOBInputDropdown` |
| `SearchMode` | `SearchMode` | `BOBInputDropdown` |
| `SearchPlaceholder` | `string` | `BOBInputDropdown` |
| `SelectAllText` | `string` | `BOBInputDropdown` |
| `Shadow` | `ShadowStyle` | `BOBInputDropdown` |
| `ShowSelectAll` | `bool` | `BOBInputDropdown` |
| `Size` | `BOBSize` | `BOBInputDropdown` |
| `Value` | `TValue` | `BOBInputDropdown` |
| `ValueChanged` | `EventCallback<TValue>` | `BOBInputDropdown` |
| `ValueExpression` | `Expression<Func<TValue>>` | `BOBInputDropdown` |
| `Variant` | `BOBInputVariant` | `BOBInputDropdown` |

## `BOBInputDropdownTree<TItem, TValue>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AnimateExpand` | `bool` | `BOBInputDropdownTree` |
| `BackgroundColor` | `string` | `BOBInputDropdownTree` |
| `ChildrenSelector` | `Func<TItem, IEnumerable<TItem>>` | `BOBInputDropdownTree` |
| `CloseOnSelect` | `bool` | `BOBInputDropdownTree` |
| `Color` | `string` | `BOBInputDropdownTree` |
| `Disabled` | `bool` | `BOBInputDropdownTree` |
| `DisplayMode` | `TreeDisplayMode` | `BOBInputDropdownTree` |
| `DisplayTextSelector` | `Func<TItem, string>` | `BOBInputDropdownTree` |
| `ExpandAll` | `bool` | `BOBInputDropdownTree` |
| `ExpandedKeys` | `HashSet<string>` | `BOBInputDropdownTree` |
| `ExpandedKeysChanged` | `EventCallback<HashSet<string>>` | `BOBInputDropdownTree` |
| `FullWidth` | `bool` | `BOBInputDropdownTree` |
| `HasChildrenSelector` | `Func<TItem, bool>` | `BOBInputDropdownTree` |
| `HelperText` | `string` | `BOBInputDropdownTree` |
| `InheritanceSeparator` | `string` | `BOBInputDropdownTree` |
| `IsLoading` | `bool` | `BOBInputDropdownTree` |
| `Items` | `IEnumerable<TItem>` | `BOBInputDropdownTree` |
| `KeySelector` | `Func<TItem, string>` | `BOBInputDropdownTree` |
| `Label` | `string` | `BOBInputDropdownTree` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBInputDropdownTree` |
| `NodeTemplate` | `RenderFragment<TreeSelectionNode<TItem>>` | `BOBInputDropdownTree` |
| `NoOptionsTemplate` | `RenderFragment` | `BOBInputDropdownTree` |
| `OnLoadChildren` | `Func<TItem, Task<IEnumerable<TItem>>>` | `BOBInputDropdownTree` |
| `Placeholder` | `string` | `BOBInputDropdownTree` |
| `Placement` | `DropdownPlacement` | `BOBInputDropdownTree` |
| `ReadOnly` | `bool` | `BOBInputDropdownTree` |
| `Required` | `bool` | `BOBInputDropdownTree` |
| `SelectionCascade` | `TreeSelectionCascade` | `BOBInputDropdownTree` |
| `Shadow` | `ShadowStyle` | `BOBInputDropdownTree` |
| `ShowCheckboxes` | `bool` | `BOBInputDropdownTree` |
| `Size` | `BOBSize` | `BOBInputDropdownTree` |
| `Value` | `TValue` | `BOBInputDropdownTree` |
| `ValueChanged` | `EventCallback<TValue>` | `BOBInputDropdownTree` |
| `ValueExpression` | `Expression<Func<TValue>>` | `BOBInputDropdownTree` |
| `Variant` | `BOBInputVariant` | `BOBInputDropdownTree` |

## `BOBInputLoading`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `Loading` | `bool` | `BOBInputLoading` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBInputLoading` |
| `Size` | `BOBSize` | `BOBInputLoading` |

## `BOBInputNumber<TValue>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<TValue, BOBInputNumber<TValue>, BOBInputVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasDensity`, `IHasDisabled`, `IHasError`, `IHasLoading`, `IHasPrefix`, `IHasReadOnly`, `IHasRequired`, `IHasShadow`, `IHasSize`, `IHasSuffix`, `IInputFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AccelerationInitialDelay` | `int` | `BOBInputNumber` |
| `AccelerationMinDelay` | `int` | `BOBInputNumber` |
| `AccelerationStepReduction` | `int` | `BOBInputNumber` |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `AllowNegative` | `bool` | `BOBInputNumber` |
| `BackgroundColor` | `string` | `BOBInputNumber` |
| `ButtonPlacement` | `StepButtonPlacement` | `BOBInputNumber` |
| `Color` | `string` | `BOBInputNumber` |
| `Culture` | `CultureInfo` | `BOBInputNumber` |
| `DecimalPlaces` | `int?` | `BOBInputNumber` |
| `Density` | `BOBDensity` | `BOBInputNumber` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `EnableAcceleration` | `bool` | `BOBInputNumber` |
| `EnableMouseWheel` | `bool` | `BOBInputNumber` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `HelperText` | `string` | `BOBInputNumber` |
| `Label` | `string` | `BOBInputNumber` |
| `Loading` | `bool` | `BOBInputNumber` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBInputNumber` |
| `Max` | `decimal?` | `BOBInputNumber` |
| `Min` | `decimal?` | `BOBInputNumber` |
| `OnDecrement` | `EventCallback<TValue>` | `BOBInputNumber` |
| `OnIncrement` | `EventCallback<TValue>` | `BOBInputNumber` |
| `Placeholder` | `string` | `BOBInputNumber` |
| `PrefixBackgroundColor` | `string` | `BOBInputNumber` |
| `PrefixColor` | `string` | `BOBInputNumber` |
| `PrefixIcon` | `IconKey?` | `BOBInputNumber` |
| `PrefixText` | `string` | `BOBInputNumber` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Shadow` | `ShadowStyle` | `BOBInputNumber` |
| `ShowStepButtons` | `bool` | `BOBInputNumber` |
| `Size` | `BOBSize` | `BOBInputNumber` |
| `Step` | `decimal` | `BOBInputNumber` |
| `SuffixBackgroundColor` | `string` | `BOBInputNumber` |
| `SuffixColor` | `string` | `BOBInputNumber` |
| `SuffixIcon` | `IconKey?` | `BOBInputNumber` |
| `SuffixText` | `string` | `BOBInputNumber` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `UseThousandsSeparator` | `bool` | `BOBInputNumber` |
| `Value` | `TValue` | `InputBase` |
| `ValueChanged` | `EventCallback<TValue>` | `InputBase` |
| `ValueExpression` | `Expression<Func<TValue>>` | `InputBase` |
| `Variant` | `BOBInputVariant` | `BOBInputComponentBase` |

## `BOBInputNumberSlider<TValue>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<TValue, BOBInputNumberSlider<TValue>, BOBInputVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasDensity`, `IHasDisabled`, `IHasError`, `IHasFullWidth`, `IHasReadOnly`, `IHasRequired`, `IHasShadow`, `IHasSize`, `IInputFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `AriaLabel` | `string` | `BOBInputNumberSlider` |
| `BackgroundColor` | `string` | `BOBInputNumberSlider` |
| `Color` | `string` | `BOBInputNumberSlider` |
| `Culture` | `CultureInfo` | `BOBInputNumberSlider` |
| `Density` | `BOBDensity` | `BOBInputNumberSlider` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `FullWidth` | `bool` | `BOBInputNumberSlider` |
| `HelperText` | `string` | `BOBInputNumberSlider` |
| `Label` | `string` | `BOBInputNumberSlider` |
| `Max` | `TValue` | `BOBInputNumberSlider` |
| `Min` | `TValue` | `BOBInputNumberSlider` |
| `Orientation` | `BOBSliderOrientation` | `BOBInputNumberSlider` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Shadow` | `ShadowStyle` | `BOBInputNumberSlider` |
| `ShowTicks` | `bool` | `BOBInputNumberSlider` |
| `ShowValueLabel` | `bool` | `BOBInputNumberSlider` |
| `Size` | `BOBSize` | `BOBInputNumberSlider` |
| `Step` | `TValue` | `BOBInputNumberSlider` |
| `TickInterval` | `TValue?` | `BOBInputNumberSlider` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `Value` | `TValue` | `InputBase` |
| `ValueChanged` | `EventCallback<TValue>` | `InputBase` |
| `ValueExpression` | `Expression<Func<TValue>>` | `InputBase` |
| `ValueLabelFormat` | `string` | `BOBInputNumberSlider` |
| `Variant` | `BOBInputVariant` | `BOBInputComponentBase` |

## `BOBInputOutline`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `For` | `string` | `BOBInputOutline` |
| `Label` | `string` | `BOBInputOutline` |
| `Required` | `bool` | `BOBInputOutline` |

## `BOBInputPrefix`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `PrefixIcon` | `IconKey?` | `BOBInputPrefix` |
| `PrefixText` | `string` | `BOBInputPrefix` |
| `Size` | `BOBSize` | `BOBInputPrefix` |

## `BOBInputRadio<TValue>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<TValue, BOBInputRadio<TValue>, BOBInputRadioVariant>`
- **Implements**: `IHasColor`, `IHasDisabled`, `IHasError`, `IHasReadOnly`, `IHasRequired`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `CheckedIcon` | `IconKey?` | `BOBInputRadio` |
| `ChildContent` | `RenderFragment` | `BOBInputRadio` |
| `Clearable` | `bool` | `BOBInputRadio` |
| `ClearText` | `string` | `BOBInputRadio` |
| `Color` | `string` | `BOBInputRadio` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `HelperText` | `string` | `BOBInputRadio` |
| `Label` | `string` | `BOBInputRadio` |
| `Orientation` | `RadioOrientation` | `BOBInputRadio` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Size` | `BOBSize` | `BOBInputRadio` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `UncheckedIcon` | `IconKey?` | `BOBInputRadio` |
| `Value` | `TValue` | `InputBase` |
| `ValueChanged` | `EventCallback<TValue>` | `InputBase` |
| `ValueExpression` | `Expression<Func<TValue>>` | `InputBase` |
| `Variant` | `BOBInputRadioVariant` | `BOBInputComponentBase` |

## `BOBInputRangeSlider<TValue>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<BOBNumericRange<TValue>, BOBInputRangeSlider<TValue>, BOBInputVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasDensity`, `IHasDisabled`, `IHasError`, `IHasFullWidth`, `IHasReadOnly`, `IHasRequired`, `IHasShadow`, `IHasSize`, `IInputFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `AriaLabelMax` | `string` | `BOBInputRangeSlider` |
| `AriaLabelMin` | `string` | `BOBInputRangeSlider` |
| `BackgroundColor` | `string` | `BOBInputRangeSlider` |
| `Color` | `string` | `BOBInputRangeSlider` |
| `Culture` | `CultureInfo` | `BOBInputRangeSlider` |
| `Density` | `BOBDensity` | `BOBInputRangeSlider` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `FullWidth` | `bool` | `BOBInputRangeSlider` |
| `HelperText` | `string` | `BOBInputRangeSlider` |
| `Label` | `string` | `BOBInputRangeSlider` |
| `Max` | `TValue` | `BOBInputRangeSlider` |
| `Min` | `TValue` | `BOBInputRangeSlider` |
| `Orientation` | `BOBSliderOrientation` | `BOBInputRangeSlider` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Shadow` | `ShadowStyle` | `BOBInputRangeSlider` |
| `ShowTicks` | `bool` | `BOBInputRangeSlider` |
| `ShowValueLabel` | `bool` | `BOBInputRangeSlider` |
| `Size` | `BOBSize` | `BOBInputRangeSlider` |
| `Step` | `TValue` | `BOBInputRangeSlider` |
| `TickInterval` | `TValue?` | `BOBInputRangeSlider` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `Value` | `BOBNumericRange<TValue>` | `InputBase` |
| `ValueChanged` | `EventCallback<BOBNumericRange<TValue>>` | `InputBase` |
| `ValueExpression` | `Expression<Func<BOBNumericRange<TValue>>>` | `InputBase` |
| `ValueLabelFormat` | `string` | `BOBInputRangeSlider` |
| `Variant` | `BOBInputVariant` | `BOBInputComponentBase` |

## `BOBInputSuffix`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `Size` | `BOBSize` | `BOBInputSuffix` |
| `SuffixIcon` | `IconKey?` | `BOBInputSuffix` |
| `SuffixText` | `string` | `BOBInputSuffix` |

## `BOBInputSwitch`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<bool, BOBInputSwitch, BOBInputSwitchVariant>`
- **Implements**: `IHasDensity`, `IHasDisabled`, `IHasError`, `IHasReadOnly`, `IHasRequired`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `Density` | `BOBDensity` | `BOBInputSwitch` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `HelperText` | `string` | `BOBInputSwitch` |
| `IconActive` | `IconKey?` | `BOBInputSwitch` |
| `IconInactive` | `IconKey?` | `BOBInputSwitch` |
| `Label` | `string` | `BOBInputSwitch` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Size` | `BOBSize` | `BOBInputSwitch` |
| `TrackColorActive` | `string` | `BOBInputSwitch` |
| `TrackColorInactive` | `string` | `BOBInputSwitch` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `Value` | `bool` | `InputBase` |
| `ValueChanged` | `EventCallback<bool>` | `InputBase` |
| `ValueExpression` | `Expression<Func<bool>>` | `InputBase` |
| `Variant` | `BOBInputSwitchVariant` | `BOBInputComponentBase` |

## `BOBInputText`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<string, BOBInputText, BOBInputVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasDensity`, `IHasDisabled`, `IHasError`, `IHasLoading`, `IHasPrefix`, `IHasReadOnly`, `IHasRequired`, `IHasShadow`, `IHasSize`, `IHasSuffix`, `IInputFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `BackgroundColor` | `string` | `BOBInputText` |
| `Color` | `string` | `BOBInputText` |
| `Density` | `BOBDensity` | `BOBInputText` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `HelperText` | `string` | `BOBInputText` |
| `Label` | `string` | `BOBInputText` |
| `Loading` | `bool` | `BOBInputText` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBInputText` |
| `OnInput` | `EventCallback<string>` | `BOBInputText` |
| `OnInputDebounceMs` | `int` | `BOBInputText` |
| `Placeholder` | `string` | `BOBInputText` |
| `PrefixBackgroundColor` | `string` | `BOBInputText` |
| `PrefixColor` | `string` | `BOBInputText` |
| `PrefixIcon` | `IconKey?` | `BOBInputText` |
| `PrefixText` | `string` | `BOBInputText` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Shadow` | `ShadowStyle` | `BOBInputText` |
| `Size` | `BOBSize` | `BOBInputText` |
| `SuffixBackgroundColor` | `string` | `BOBInputText` |
| `SuffixColor` | `string` | `BOBInputText` |
| `SuffixIcon` | `IconKey?` | `BOBInputText` |
| `SuffixText` | `string` | `BOBInputText` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `UpdateOnInput` | `bool` | `BOBInputText` |
| `Value` | `string` | `InputBase` |
| `ValueChanged` | `EventCallback<string>` | `InputBase` |
| `ValueExpression` | `Expression<Func<string>>` | `InputBase` |
| `Variant` | `BOBInputVariant` | `BOBInputComponentBase` |

## `BOBInputTextArea`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBInputComponentBase<string, BOBInputTextArea, BOBInputVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasDensity`, `IHasDisabled`, `IHasError`, `IHasLoading`, `IHasPrefix`, `IHasReadOnly`, `IHasRequired`, `IHasShadow`, `IHasSize`, `IHasSuffix`, `IInputFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `InputBase` |
| `AutoResize` | `bool` | `BOBInputTextArea` |
| `BackgroundColor` | `string` | `BOBInputTextArea` |
| `Color` | `string` | `BOBInputTextArea` |
| `Density` | `BOBDensity` | `BOBInputTextArea` |
| `Disabled` | `bool` | `BOBInputComponentBase` |
| `DisplayName` | `string` | `InputBase` |
| `Error` | `bool` | `BOBInputComponentBase` |
| `HelperText` | `string` | `BOBInputTextArea` |
| `Label` | `string` | `BOBInputTextArea` |
| `Loading` | `bool` | `BOBInputTextArea` |
| `LoadingIndicatorVariant` | `BOBLoadingIndicatorVariant` | `BOBInputTextArea` |
| `MaxLength` | `int?` | `BOBInputTextArea` |
| `OnInput` | `EventCallback<string>` | `BOBInputTextArea` |
| `OnInputDebounceMs` | `int` | `BOBInputTextArea` |
| `Placeholder` | `string` | `BOBInputTextArea` |
| `PrefixBackgroundColor` | `string` | `BOBInputTextArea` |
| `PrefixColor` | `string` | `BOBInputTextArea` |
| `PrefixIcon` | `IconKey?` | `BOBInputTextArea` |
| `PrefixText` | `string` | `BOBInputTextArea` |
| `ReadOnly` | `bool` | `BOBInputComponentBase` |
| `Required` | `bool` | `BOBInputComponentBase` |
| `Resize` | `TextAreaResize` | `BOBInputTextArea` |
| `Rows` | `int` | `BOBInputTextArea` |
| `Shadow` | `ShadowStyle` | `BOBInputTextArea` |
| `Size` | `BOBSize` | `BOBInputTextArea` |
| `SuffixBackgroundColor` | `string` | `BOBInputTextArea` |
| `SuffixColor` | `string` | `BOBInputTextArea` |
| `SuffixIcon` | `IconKey?` | `BOBInputTextArea` |
| `SuffixText` | `string` | `BOBInputTextArea` |
| `TrackPerformanceEnabled` | `bool` | `BOBInputComponentBase` |
| `UpdateOnInput` | `bool` | `BOBInputTextArea` |
| `Value` | `string` | `InputBase` |
| `ValueChanged` | `EventCallback<string>` | `InputBase` |
| `ValueExpression` | `Expression<Func<string>>` | `InputBase` |
| `Variant` | `BOBInputVariant` | `BOBInputComponentBase` |

## `BOBLoadingIndicator`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBVariantComponentBase<BOBLoadingIndicator, BOBLoadingIndicatorVariant>`
- **Implements**: `IHasColor`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `AriaLabel` | `string` | `BOBLoadingIndicator` |
| `Color` | `string` | `BOBLoadingIndicator` |
| `Size` | `BOBSize` | `BOBLoadingIndicator` |
| `Variant` | `BOBLoadingIndicatorVariant` | `BOBVariantComponentBase` |

## `BOBModalContainer`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `Modal` | `ModalState` | `BOBModalContainer` |

## `BOBModalHost`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `ComponentBase`

## `BOBNotificationBadge`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBVariantComponentBase<BOBNotificationBadge, BOBBadgeVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `BackgroundColor` | `string` | `BOBNotificationBadge` |
| `BadgeContent` | `RenderFragment` | `BOBNotificationBadge` |
| `Border` | `BorderStyle` | `BOBNotificationBadge` |
| `ChildContent` | `RenderFragment` | `BOBNotificationBadge` |
| `Circular` | `bool` | `BOBNotificationBadge` |
| `Color` | `string` | `BOBNotificationBadge` |
| `Position` | `BadgePosition` | `BOBNotificationBadge` |
| `Shadow` | `ShadowStyle` | `BOBNotificationBadge` |
| `Size` | `BOBSize` | `BOBNotificationBadge` |
| `Variant` | `BOBBadgeVariant` | `BOBVariantComponentBase` |

## `BOBPerformanceDashboard`

- **Namespace**: `BlazOrbit.Components.Diagnostics`
- **Base**: `ComponentBase`

## `BOBSelect<TValue>`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBVariantComponentBase<BOBSelect<TValue>, BOBSelectVariant>`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasDisabled`, `IHasFullWidth`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `BackgroundColor` | `string` | `BOBSelect` |
| `ChildContent` | `RenderFragment` | `BOBSelect` |
| `Color` | `string` | `BOBSelect` |
| `Disabled` | `bool` | `BOBSelect` |
| `FullWidth` | `bool` | `BOBSelect` |
| `HelperText` | `string` | `BOBSelect` |
| `Label` | `string` | `BOBSelect` |
| `ReadOnly` | `bool` | `BOBSelect` |
| `Required` | `bool` | `BOBSelect` |
| `Size` | `BOBSize` | `BOBSelect` |
| `Value` | `TValue` | `BOBSelect` |
| `ValueChanged` | `EventCallback<TValue>` | `BOBSelect` |
| `Variant` | `BOBSelectVariant` | `BOBVariantComponentBase` |

## `BOBSidebarLayout`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `ChildContent` | `RenderFragment` | `BOBSidebarLayout` |
| `CollapseBreakpoint` | `string` | `BOBSidebarLayout` |
| `ContentMaxWidth` | `string` | `BOBSidebarLayout` |
| `Header` | `RenderFragment` | `BOBSidebarLayout` |
| `HeaderHeight` | `string` | `BOBSidebarLayout` |
| `ShowToggle` | `bool` | `BOBSidebarLayout` |
| `Sidebar` | `RenderFragment` | `BOBSidebarLayout` |
| `SidebarOpen` | `bool` | `BOBSidebarLayout` |
| `SidebarOpenChanged` | `EventCallback<bool>` | `BOBSidebarLayout` |
| `SidebarSide` | `SidebarSide` | `BOBSidebarLayout` |
| `SidebarWidth` | `string` | `BOBSidebarLayout` |
| `StickyHeader` | `bool` | `BOBSidebarLayout` |
| `StickySidebar` | `bool` | `BOBSidebarLayout` |

## `BOBStackedLayout`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `ChildContent` | `RenderFragment` | `BOBStackedLayout` |
| `ContentMaxWidth` | `string` | `BOBStackedLayout` |
| `Header` | `RenderFragment` | `BOBStackedLayout` |
| `HeaderHeight` | `string` | `BOBStackedLayout` |
| `Nav` | `RenderFragment` | `BOBStackedLayout` |
| `NavColumns` | `int?` | `BOBStackedLayout` |
| `NavGap` | `string` | `BOBStackedLayout` |
| `NavMinColumnWidth` | `string` | `BOBStackedLayout` |
| `NavOpen` | `bool` | `BOBStackedLayout` |
| `NavOpenChanged` | `EventCallback<bool>` | `BOBStackedLayout` |
| `ShowToggle` | `bool` | `BOBStackedLayout` |
| `StickyHeader` | `bool` | `BOBStackedLayout` |
| `StickyNav` | `bool` | `BOBStackedLayout` |

## `BOBSvgIcon`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBVariantComponentBase<BOBSvgIcon, BOBSvgIconVariant>`
- **Implements**: `IHasColor`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `Color` | `string` | `BOBSvgIcon` |
| `Icon` | `IconKey` | `BOBSvgIcon` |
| `OnClick` | `EventCallback<MouseEventArgs>` | `BOBSvgIcon` |
| `Size` | `BOBSize` | `BOBSvgIcon` |
| `Title` | `string` | `BOBSvgIcon` |
| `Variant` | `BOBSvgIconVariant` | `BOBVariantComponentBase` |
| `ViewBox` | `string` | `BOBSvgIcon` |

## `BOBSwitch<TValue>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasDensity`, `IHasDisabled`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `AriaDescribedBy` | `string` | `BOBSwitch` |
| `AriaLabel` | `string` | `BOBSwitch` |
| `Density` | `BOBDensity` | `BOBSwitch` |
| `Disabled` | `bool` | `BOBSwitch` |
| `IconActive` | `IconKey?` | `BOBSwitch` |
| `IconInactive` | `IconKey?` | `BOBSwitch` |
| `InputId` | `string` | `BOBSwitch` |
| `Label` | `string` | `BOBSwitch` |
| `OptionActive` | `TValue` | `BOBSwitch` |
| `OptionInactive` | `TValue` | `BOBSwitch` |
| `Size` | `BOBSize` | `BOBSwitch` |
| `ThumbBackgroundColorActive` | `string` | `BOBSwitch` |
| `ThumbBackgroundColorInactive` | `string` | `BOBSwitch` |
| `ThumbColorActive` | `string` | `BOBSwitch` |
| `ThumbColorInactive` | `string` | `BOBSwitch` |
| `TrackColorActive` | `string` | `BOBSwitch` |
| `TrackColorInactive` | `string` | `BOBSwitch` |
| `Value` | `TValue` | `BOBSwitch` |
| `ValueChanged` | `EventCallback<TValue>` | `BOBSwitch` |

## `BOBTab`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ChildContent` | `RenderFragment` | `BOBTab` |
| `Disabled` | `bool` | `BOBTab` |
| `Icon` | `RenderFragment` | `BOBTab` |
| `Id` | `string` | `BOBTab` |
| `Label` | `string` | `BOBTab` |

## `BOBTabs`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBVariantComponentBase<BOBTabs, BOBTabsVariant>`
- **Implements**: `IHasFullWidth`, `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ActiveTab` | `string` | `BOBTabs` |
| `ActiveTabChanged` | `EventCallback<string>` | `BOBTabs` |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `ChildContent` | `RenderFragment` | `BOBTabs` |
| `FullWidth` | `bool` | `BOBTabs` |
| `Size` | `BOBSize` | `BOBTabs` |
| `Variant` | `BOBTabsVariant` | `BOBVariantComponentBase` |

## `BOBThemeSelector`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBVariantComponentBase<BOBThemeSelector, BOBThemeSelectorVariant>`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `AvailableThemes` | `string[]` | `BOBThemeSelector` |
| `OnThemeChanged` | `EventCallback<string>` | `BOBThemeSelector` |
| `ShowIcon` | `bool` | `BOBThemeSelector` |
| `ThemeIcons` | `Dictionary<string, string>` | `BOBThemeSelector` |
| `Variant` | `BOBThemeSelectorVariant` | `BOBVariantComponentBase` |

## `BOBTimePicker`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasDensity`, `IHasSize`, `IPickerFamilyComponent`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `Density` | `BOBDensity` | `BOBTimePicker` |
| `Size` | `BOBSize` | `BOBTimePicker` |
| `Value` | `TimeOnly?` | `BOBTimePicker` |
| `ValueChanged` | `EventCallback<TimeOnly?>` | `BOBTimePicker` |

## `BOBToast`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBVariantComponentBase<BOBToast, BOBToastVariant>`
- **Implements**: `IHasElevation`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `Elevation` | `int?` | `BOBToast` |
| `OnCloseAnimationComplete` | `EventCallback<Guid>` | `BOBToast` |
| `State` | `ToastState` | `BOBToast` |
| `Variant` | `BOBToastVariant` | `BOBVariantComponentBase` |

## `BOBToastHost`

- **Namespace**: `BlazOrbit.Components.Layout`
- **Base**: `BOBComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `MaxVisiblePerPosition` | `int` | `BOBToastHost` |

## `BOBTooltip`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasBackgroundColor`, `IHasColor`, `IHasElevation`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `Arrow` | `bool` | `BOBTooltip` |
| `AutoCloseAfterMs` | `int?` | `BOBTooltip` |
| `BackgroundColor` | `string` | `BOBTooltip` |
| `ChildContent` | `RenderFragment` | `BOBTooltip` |
| `Color` | `string` | `BOBTooltip` |
| `Elevation` | `int?` | `BOBTooltip` |
| `Interactive` | `bool` | `BOBTooltip` |
| `LeaveDelay` | `int?` | `BOBTooltip` |
| `MaxWidth` | `int?` | `BOBTooltip` |
| `Offset` | `int` | `BOBTooltip` |
| `Placement` | `TooltipPlacement` | `BOBTooltip` |
| `ShowDelay` | `int?` | `BOBTooltip` |
| `Text` | `string` | `BOBTooltip` |
| `TooltipContent` | `RenderFragment` | `BOBTooltip` |
| `Trigger` | `TooltipTrigger` | `BOBTooltip` |

## `BOBTreeMenu<TItem>`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `Cache` | `TreeNodeCache<TItem>` | `BOBTreeMenu` |
| `ChildContent` | `RenderFragment` | `BOBTreeMenu` |
| `ChildrenSelector` | `Func<TItem, IEnumerable<TItem>>` | `BOBTreeMenu` |
| `Collapsible` | `bool` | `BOBTreeMenu` |
| `ExpandAll` | `bool` | `BOBTreeMenu` |
| `ExpandedKeys` | `HashSet<string>` | `BOBTreeMenu` |
| `ExpandedKeysChanged` | `EventCallback<HashSet<string>>` | `BOBTreeMenu` |
| `ExpandMode` | `TreeMenuExpandMode` | `BOBTreeMenu` |
| `HasChildrenSelector` | `Func<TItem, bool>` | `BOBTreeMenu` |
| `HoverCloseDelay` | `int` | `BOBTreeMenu` |
| `HoverOpenDelay` | `int` | `BOBTreeMenu` |
| `Items` | `IEnumerable<TItem>` | `BOBTreeMenu` |
| `KeySelector` | `Func<TItem, string>` | `BOBTreeMenu` |
| `NodeTemplate` | `RenderFragment<TreeMenuNode<TItem>>` | `BOBTreeMenu` |
| `OnLoadChildren` | `Func<TItem, Task<IEnumerable<TItem>>>` | `BOBTreeMenu` |
| `OnNavigate` | `EventCallback<string>` | `BOBTreeMenu` |
| `OnNodeClick` | `EventCallback<TreeNodeEventArgs<TreeMenuNode<TItem>>>` | `BOBTreeMenu` |
| `Orientation` | `TreeMenuOrientation` | `BOBTreeMenu` |
| `Size` | `BOBSize` | `BOBTreeMenu` |
| `Trigger` | `TreeMenuTrigger` | `BOBTreeMenu` |

## `BOBTreeMenuItem`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBTreeNodeBase<TreeMenuNodeRegistration>`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ChildContent` | `RenderFragment` | `BOBTreeNodeBase` |
| `Data` | `object` | `BOBTreeNodeBase` |
| `Disabled` | `bool` | `BOBTreeNodeBase` |
| `Href` | `string` | `BOBTreeMenuItem` |
| `Icon` | `IconKey?` | `BOBTreeNodeBase` |
| `InitiallyExpanded` | `bool` | `BOBTreeNodeBase` |
| `Key` | `string` | `BOBTreeNodeBase` |
| `Match` | `NavLinkMatch` | `BOBTreeMenuItem` |
| `NodeContent` | `RenderFragment` | `BOBTreeNodeBase` |
| `OnClick` | `EventCallback` | `BOBTreeMenuItem` |
| `Target` | `string` | `BOBTreeMenuItem` |
| `Text` | `string` | `BOBTreeNodeBase` |

## `BOBTreeSelector<TItem>`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBComponentBase`
- **Implements**: `IHasSize`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>` | `BOBComponentBase` |
| `AnimateExpand` | `bool` | `BOBTreeSelector` |
| `Cache` | `TreeNodeCache<TItem>` | `BOBTreeSelector` |
| `ChildContent` | `RenderFragment` | `BOBTreeSelector` |
| `ChildrenSelector` | `Func<TItem, IEnumerable<TItem>>` | `BOBTreeSelector` |
| `CollapseIcon` | `IconKey` | `BOBTreeSelector` |
| `DisplayTextSelector` | `Func<TItem, string>` | `BOBTreeSelector` |
| `ExpandAll` | `bool` | `BOBTreeSelector` |
| `ExpandedKeys` | `HashSet<string>` | `BOBTreeSelector` |
| `ExpandedKeysChanged` | `EventCallback<HashSet<string>>` | `BOBTreeSelector` |
| `ExpandIcon` | `IconKey` | `BOBTreeSelector` |
| `HasChildrenSelector` | `Func<TItem, bool>` | `BOBTreeSelector` |
| `Items` | `IEnumerable<TItem>` | `BOBTreeSelector` |
| `KeySelector` | `Func<TItem, string>` | `BOBTreeSelector` |
| `NodeTemplate` | `RenderFragment<TreeSelectionNode<TItem>>` | `BOBTreeSelector` |
| `OnLoadChildren` | `Func<TItem, Task<IEnumerable<TItem>>>` | `BOBTreeSelector` |
| `OnNodeClick` | `EventCallback<TreeNodeEventArgs<TreeSelectionNode<TItem>>>` | `BOBTreeSelector` |
| `OnNodeCollapse` | `EventCallback<TreeNodeEventArgs<TreeSelectionNode<TItem>>>` | `BOBTreeSelector` |
| `OnNodeExpand` | `EventCallback<TreeNodeEventArgs<TreeSelectionNode<TItem>>>` | `BOBTreeSelector` |
| `SelectedKeys` | `HashSet<string>` | `BOBTreeSelector` |
| `SelectedKeysChanged` | `EventCallback<HashSet<string>>` | `BOBTreeSelector` |
| `SelectionMode` | `TreeSelectionMode` | `BOBTreeSelector` |
| `ShowCheckboxes` | `bool` | `BOBTreeSelector` |
| `Size` | `BOBSize` | `BOBTreeSelector` |

## `BOBTreeSelectorItem`

- **Namespace**: `BlazOrbit.Components`
- **Base**: `BOBTreeNodeBase<TreeSelectionNodeRegistration>`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ChildContent` | `RenderFragment` | `BOBTreeNodeBase` |
| `Data` | `object` | `BOBTreeNodeBase` |
| `Disabled` | `bool` | `BOBTreeNodeBase` |
| `Icon` | `IconKey?` | `BOBTreeNodeBase` |
| `InitiallyExpanded` | `bool` | `BOBTreeNodeBase` |
| `Key` | `string` | `BOBTreeNodeBase` |
| `NodeContent` | `RenderFragment` | `BOBTreeNodeBase` |
| `Text` | `string` | `BOBTreeNodeBase` |

## `DropdownOption<TOption>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ChildContent` | `RenderFragment` | `DropdownOption` |
| `Disabled` | `bool` | `DropdownOption` |
| `Text` | `string` | `DropdownOption` |
| `Value` | `TOption` | `DropdownOption` |

## `RadioOption<TOption>`

- **Namespace**: `BlazOrbit.Components.Forms`
- **Base**: `ComponentBase`

### Parameters

| Parameter | Type | Declared on |
|-----------|------|-------------|
| `ChildContent` | `RenderFragment` | `RadioOption` |
| `Disabled` | `bool` | `RadioOption` |
| `Value` | `TOption` | `RadioOption` |

