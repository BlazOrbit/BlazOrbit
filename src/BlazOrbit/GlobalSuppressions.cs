// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

#region RS0041 - BuildRenderTree oblivious reference types (Blazor)

// Blazor generates a BuildRenderTree method that references RenderTreeBuilder from the pre-nullable Microsoft.AspNetCore.Components package.
// This causes RS0041 to be raised because the public API file expects all references to be nullable-aware.
// Suppress this warning for the BuildRenderTree method as it is already correctly added in the public API file and cannot be changed to reference
// a nullable-aware type without breaking Blazor's code generation.

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBBadge.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBButton.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBCodeBlock.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBDataCards`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBDataColumn`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBDataGrid`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBDraggable.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBLoadingIndicator.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBNotificationBadge.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBSelect`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBSvgIcon.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBTab.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBTabs.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBTooltip.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBTreeMenuItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBTreeMenu`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBTreeSelectorItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBTreeSelector`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Diagnostics.BOBPerformanceDashboard.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBColorPicker.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBDatePicker.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputCheckbox`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputColor.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputDateTime`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputDropdownTree`2.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputDropdown`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputLoading.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputNumber`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputOutline.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputPrefix.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputRadio`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputSuffix.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputSwitch.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputText.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputTextArea.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBSwitch`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBTimePicker.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.Dropdown.BOBDropdownContainer`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Internal._BOBAddon.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Internal._BOBBtn.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Internal._BOBCheckMark.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Internal._BOBFieldHelper`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Internal._BOBInNumber.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Internal._BOBInSelect.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Internal._BOBInText.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Internal._BOBPagination.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBBlazorLayout.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBCard.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBDialog.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBDrawer.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBFlexStack.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBGrid.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBGridItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBInitializer.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBModalContainer.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBModalHost.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBSidebarLayout.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBStackedLayout.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBThemeSelector.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBToast.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Layout.BOBToastHost.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Utils.BOBDateTimePattern.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.Internal._BOBSliderTrack.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.Internal._BOBSliderThumb.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputNumberSlider`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.Forms.BOBInputRangeSlider`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBAccordion.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBAccordionItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBCarousel.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target = "~M:BlazOrbit.Components.BOBCarouselItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

#endregion
