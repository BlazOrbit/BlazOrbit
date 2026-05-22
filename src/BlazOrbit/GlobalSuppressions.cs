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
    Target =
        "~M:BlazOrbit.Components.BOBBadge.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBButton.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBDataCards`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBDataColumn`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBDataGrid`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBDraggable.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBProgressIcon.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBNotificationBadge.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBSelect`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBSvgIcon.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBTab.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBTabs.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBTooltip.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBTreeMenuItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBTreeMenu`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBTreeSelectorItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBTreeSelector`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBPerformanceDashboard.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBAvatar.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBAvatarGroup.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBBanner.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBChip.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBBreadcrumbs.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBTimeline.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBStepper.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBStep.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBAspectRatio.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBPageHeader.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBContainer.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBSection.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBSplitter.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBSplitterPane.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBConfirmDialog.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBRating.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBStatCard.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBProgressBar.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBProgressRing.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBAutoComplete`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBColorPicker.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBDatePicker.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputCheckbox`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputDateRange.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputFile.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputColor.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputDateTime`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputDropdownTree`2.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputDropdown`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputLoading.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputNumber`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputOtp.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputPassword.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputOutline.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputPrefix.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputRadio`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputSuffix.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputSwitch.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputText.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputTextArea.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBSwitch`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBTimePicker.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.Dropdown.BOBDropdownContainer`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBInAddon.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBInBtn.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBInCheckMark.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBFieldHelper`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBInNumber.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBInSelect.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBInDate.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBDateRangePicker.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBChipGroup`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBInText.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBInPagination.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBBlazorLayout.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBCard.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBDialog.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBDrawer.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBFlexStack.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBGrid.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBGridItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBInitializer.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBModalContainer.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBModalHost.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBSidebarLayout.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBStackedLayout.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBThemeSelector.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBToast.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBToastHost.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBDateTimePattern.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.Internal._BOBSliderTrack.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.Internal._BOBSliderThumb.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputNumberSlider`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.Forms.BOBInputRangeSlider`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBAccordion.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBAccordionItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBCarousel.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components.BOBCarouselItem.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

[assembly: SuppressMessage(
    "Microsoft.CodeAnalysis.PublicApiAnalyzers",
    "RS0041:PublicApiFilesInvalid",
    Justification = "BuildRenderTree uses RenderTreeBuilder from pre-nullable Microsoft.AspNetCore.Components",
    Scope = "member",
    Target =
        "~M:BlazOrbit.Components._BOBDataGridCellEditor`1.BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder)")]

#endregion