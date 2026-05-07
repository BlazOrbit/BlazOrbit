<!-- handcrafted: do NOT regenerate. The regenerator only writes
     components.md / variants.md / icons.md. -->

# Common Patterns

Copy-paste examples with realistic parameter density. Each pattern exercises a meaningful
subset of the component's surface so an agent can pattern-match without round-tripping to
`components.md`. For the full parameter list per component, see that file.

## App bootstrap

```csharp
// Program.cs
using BlazOrbit;
using BlazOrbit.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBlazOrbit();

// Optional — only when registering custom variants. See `variants.md`.
// builder.Services.AddBlazOrbitVariants(b =>
//     b.ForComponent<BOBButton>().AddVariant(MyVariants.Gradient, MyTemplates.Gradient));
```

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase

<BOBBlazorLayout>
    <BOBSidebarLayout SidebarSide="SidebarSide.Start"
                      SidebarWidth="260px"
                      HeaderHeight="56px"
                      StickyHeader="true"
                      StickySidebar="true"
                      CollapseBreakpoint="1024px">
        <Header><AppHeader /></Header>
        <Sidebar><AppNav /></Sidebar>
        <ChildContent>@Body</ChildContent>
    </BOBSidebarLayout>
</BOBBlazorLayout>
```

`BOBBlazorLayout` auto-mounts `BOBInitializer`, `BOBModalHost`, and `BOBToastHost`. If you
write your own root layout, mount those three yourself.

## Button — full surface

```razor
<BOBButton Text="Save changes"
           LeadingIcon="@BOBIconKeys.UI.Save"
           TrailingIcon="@BOBIconKeys.UI.ChevronRight"
           Size="BOBSize.Medium"
           Density="BOBDensity.Standard"
           Variant="BOBButtonVariant.Default"
           BackgroundColor="@PaletteColor.Primary"
           Color="@PaletteColor.PrimaryContrast"
           Border="@BOBBorderPresets.Rounded"
           Shadow="@BOBShadowPresets.Elevation(2)"
           Transitions="@BOBTransitionPresets.MaterialButton"
           RippleColor="@PaletteColor.PrimaryContrast"
           RippleDurationMs="450"
           FullWidth="false"
           Loading="@_saving"
           Disabled="@(_saving || !_canSave)"
           AriaLabel="Save changes"
           OnClick="SaveAsync" />
```

## Form with validation

Inputs auto-integrate with `EditForm` / `EditContext` / validators. Helper text is
replaced with the validation message when validation fails; `aria-invalid`/`aria-required`
are emitted automatically. `BOBButton` does not currently expose `Type="submit"`, so use
a plain `<button type="submit">` for the submit action.

```razor
<EditForm Model="_model" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />

    <BOBInputText @bind-Value="_model.Name"
                  Label="Full name"
                  HelperText="As it appears on your ID"
                  Placeholder="Jane Doe"
                  Required="true"
                  Variant="BOBInputVariant.Outlined"
                  Size="BOBSize.Medium"
                  PrefixIcon="@BOBIconKeys.UI.Person" />

    <BOBInputText @bind-Value="_model.Email"
                  Label="Email"
                  Variant="BOBInputVariant.Filled"
                  Color="@PaletteColor.Primary"
                  PrefixIcon="@BOBIconKeys.UI.Email"
                  UpdateOnInput="true"
                  OnInputDebounceMs="250" />

    <BOBInputDateTime @bind-Value="_model.DateOfBirth"
                      Label="Date of birth" />

    <BOBInputCheckbox @bind-Value="_model.AcceptTerms"
                      Label="I accept the terms"
                      Required="true"
                      Color="@PaletteColor.Primary" />

    <button type="submit">Submit</button>
</EditForm>
```

## Card — header + media + actions

```razor
<BOBCard Elevation="2"
         Border="@BOBBorderPresets.RoundedLarge"
         BackgroundColor="@PaletteColor.Surface"
         Color="@PaletteColor.SurfaceContrast"
         MediaHeight="180px"
         MediaPosition="CardMediaPosition.Top"
         ActionsAlignment="CardActionsAlignment.SpaceBetween"
         Clickable="true"
         OnClick="OpenAsync">
    <Media>
        <img src="@_item.ThumbUrl" alt=""
             style="width: 100%; height: 100%; object-fit: cover;" />
    </Media>
    <Header><h3 style="margin: 0;">@_item.Title</h3></Header>
    <ChildContent>
        <p>@_item.Description</p>
    </ChildContent>
    <Actions>
        <BOBButton Text="Cancel" />
        <BOBButton Text="Confirm"
                   BackgroundColor="@PaletteColor.Primary"
                   Color="@PaletteColor.PrimaryContrast" />
    </Actions>
</BOBCard>
```

## DataGrid — columns slot, sortable + filterable

```razor
<BOBDataGrid TItem="User" Items="_users"
             Hoverable="true"
             FixedHeader="true"
             RowBorder="@BOBBorderPresets.Subtle"
             CellBorder="@BOBBorderPresets.None"
             Size="BOBSize.Medium"
             Density="BOBDensity.Compact">
    <Columns>
        <BOBDataColumn TItem="User" Property="u => u.Name"     Header="Name"     Sortable="true" Filterable="true" />
        <BOBDataColumn TItem="User" Property="u => u.Email"    Header="Email"    Sortable="true" Width="260px" />
        <BOBDataColumn TItem="User" Property="u => u.JoinedAt" Header="Joined"   Format="yyyy-MM-dd" Align="ColumnAlign.Right" />
        <BOBDataColumn TItem="User" Header="Actions" Align="ColumnAlign.Right">
            <Template Context="user">
                <BOBButton Text="Open" Size="BOBSize.Small" OnClick="() => OpenAsync(user)" />
            </Template>
        </BOBDataColumn>
    </Columns>
</BOBDataGrid>
```

## Tabs — `Id`/`Label`, bound by string id

```razor
<BOBTabs @bind-ActiveTab="_activeTab"
         Variant="BOBTabsVariant.Underline"
         Size="BOBSize.Medium"
         FullWidth="false">
    <BOBTab Id="profile"  Label="Profile">  <ProfilePane  /></BOBTab>
    <BOBTab Id="security" Label="Security"> <SecurityPane /></BOBTab>
    <BOBTab Id="billing"  Label="Billing" Disabled="true"><BillingPane /></BOBTab>
</BOBTabs>
```

## Accordion — controlled, multi-expand

```razor
<BOBAccordion Mode="BOBAccordionMode.Multiple"
              ExpandedItems="_expanded"
              ExpandedItemsChanged="v => _expanded = v"
              Border="@BOBBorderPresets.Subtle"
              Gap="4px"
              Size="BOBSize.Medium">
    <BOBAccordionItem Id="general"  Header="General"><GeneralPane  /></BOBAccordionItem>
    <BOBAccordionItem Id="advanced" Header="Advanced"><AdvancedPane /></BOBAccordionItem>
</BOBAccordion>
```

## Dialog (declarative)

`BOBDialog` slots are `Header`, `Content`, `Footer` — not `ChildContent` / `Actions`.

```razor
<BOBDialog @bind-Open="_dialogOpen"
           Title="Confirm deletion"
           MaxWidth="480px"
           Closable="true"
           CloseOnOverlayClick="true"
           CloseOnEscape="true"
           Elevation="6">
    <Content>
        <p>This will permanently remove <strong>@_target.Name</strong>.</p>
    </Content>
    <Footer>
        <BOBButton Text="Cancel" OnClick="() => _dialogOpen = false" />
        <BOBButton Text="Delete"
                   BackgroundColor="@PaletteColor.Error"
                   Color="@PaletteColor.ErrorContrast"
                   OnClick="DeleteAsync" />
    </Footer>
</BOBDialog>
```

## Drawer (declarative)

```razor
<BOBDrawer @bind-Open="_drawerOpen"
           Position="DrawerPosition.Right"
           Size="360px"
           Closable="true"
           CloseOnOverlayClick="true"
           Elevation="4">
    <Header><h3>Filters</h3></Header>
    <ChildContent><FiltersForm /></ChildContent>
    <Footer>
        <BOBButton Text="Reset" />
        <BOBButton Text="Apply"
                   BackgroundColor="@PaletteColor.Primary"
                   Color="@PaletteColor.PrimaryContrast" />
    </Footer>
</BOBDrawer>
```

## Toast (imperative)

`IToastService.ShowAsync<TBody>(...)` renders a Razor body component. There is no
`Show(string, severity)` overload — define a small body component once and reuse it.

```razor
@* Toasts/MyToast.razor *@
<div>
    <strong>@Title</strong>
    <p>@Message</p>
</div>

@code {
    [Parameter] public string? Title   { get; set; }
    [Parameter] public string? Message { get; set; }
}
```

```razor
@inject IToastService ToastService

<BOBButton Text="Save" OnClick="SaveAsync" />

@code {
    private async Task SaveAsync()
    {
        await DoWorkAsync();
        await ToastService.ShowAsync<MyToast>(
            new Dictionary<string, object?>
            {
                ["Title"]   = "Saved",
                ["Message"] = "Your changes are stored."
            },
            ToastOptions.Default with
            {
                Severity = ToastSeverity.Success,
                Position = ToastPosition.BottomRight,
                Duration = TimeSpan.FromSeconds(3)
            });
    }
}
```

## Modal (imperative)

Body component implements `IModalContent`; the host injects a close handle.

```razor
@inject IModalService ModalService

<BOBButton Text="Edit" OnClick="EditAsync" />

@code {
    private async Task EditAsync()
    {
        var result = await ModalService.ShowDialogAsync<EditUserDialog, UserDto>(
            parameters: new { UserId = _selectedId },
            options: new DialogOptions
            {
                Title = "Edit user",
                MaxWidth = "560px",
                CloseOnOverlayClick = false
            });

        if (result is not null) await ReloadAsync();
    }
}
```

## Custom variant — registration

Variant templates live in a `.razor` file (Razor markup is not legal inside
`.cs`). Use the explicit-template syntax `@<text>…</text>` so Razor can tell
the markup apart from C# inside the lambda body:

```razor
@* Templates/ButtonTemplates.razor *@
@* No @page / @inherits — this file only declares static templates. *@

@code {
    public static readonly BOBButtonVariant Gradient = BOBButtonVariant.Custom("Gradient");

    public static readonly RenderFragment<BOBButton> GradientTemplate = button => @<text>
        <bob-component @attributes="@button.ComputedAttributes">
            <button type="button"
                    class="transition-target"
                    disabled="@button.IsDisabled"
                    @onclick="async e => await button.OnClick.InvokeAsync(e)"
                    style="background: linear-gradient(135deg, var(--palette-primary), var(--palette-secondary));
                           color: var(--palette-primary-contrast); border: 0;">
                @button.LeadingIcon?.SvgContent
                @button.Text
            </button>
        </bob-component>
    </text>;
}
```

```csharp
// Program.cs
using MyApp.Templates;

builder.Services.AddBlazOrbitVariants(b =>
    b.ForComponent<BOBButton>()
     .AddVariant(ButtonTemplates.Gradient, ButtonTemplates.GradientTemplate));
```

```razor
<BOBButton Text="Save" Variant="@ButtonTemplates.Gradient" OnClick="SaveAsync" />
```

Cache the variant identifier (`ButtonTemplates.Gradient`) in a static field so
both registration and consumption reference the same instance.

## Custom variant — by inheritance

When you need additional parameters or behavior alongside the markup, subclass:

```razor
@* MyBOBButton.razor *@
@inherits BOBButton

<bob-component @attributes="ComputedAttributes">
    <button @ref="_ref"
            class="my-custom-btn transition-target"
            disabled="@IsDisabled"
            @onclick="async e => await OnClick.InvokeAsync(e)">
        @ChildContent ?? @Text
    </button>
</bob-component>

@code {
    private ElementReference _ref;
    public override ElementReference GetRippleContainer() => _ref;
}
```

## Theme override (CSS-only)

Define a theme by overriding palette + token variables under a theme selector.
`BOBInitializer` (mounted by `BOBBlazorLayout`) sets `<html data-bob-theme="<id>">`
on first load and on theme changes; `BOBThemeSelector` is the prebuilt switcher.

```css
/* wwwroot/css/themes.css */
:root {
    --bob-font-family:    "Inter", system-ui, sans-serif;
    --bob-border-radius:  8px;
    --bob-input-radius:   6px;
}

html[data-bob-theme="brand-dark"] {
    --palette-primary:           #5b8def;
    --palette-primary-contrast:  #ffffff;
    --palette-surface:           #1a1d23;
    --palette-surface-contrast:  #f1f3f7;
    --palette-background:        #11131a;
    --palette-background-contrast:#e9ecf2;
    --palette-error:             #f25f5c;
    --palette-highlight:         #5b8def;
    --bob-highlight-outline:     3px solid var(--palette-primary);
}
```

## Sidebar layout

```razor
<BOBSidebarLayout SidebarSide="SidebarSide.Start"
                  SidebarWidth="240px"
                  HeaderHeight="56px"
                  ContentMaxWidth="1200px"
                  StickyHeader="true"
                  StickySidebar="true"
                  CollapseBreakpoint="1024px">
    <Header><AppHeader /></Header>
    <Sidebar><BOBTreeMenu TItem="NavItem" Items="_navItems" /></Sidebar>
    <ChildContent>@Body</ChildContent>
</BOBSidebarLayout>
```

