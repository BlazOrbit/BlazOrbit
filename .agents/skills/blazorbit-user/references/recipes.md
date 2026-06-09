<!-- handcrafted: do NOT regenerate. The regenerator only writes
     components.md / variants.md / icons.md. -->

# Recipes — full-page patterns

Production-shaped recipes for the typical surfaces of a modern Blazor app
built with BlazOrbit. Each recipe is **whole-page**, not a single component
demo, and exercises a meaningful subset of the library so you can lift
sections wholesale into a real project.

> All snippets assume the app bootstrap from `SKILL.md → Quick start` is in
> place (`AddBlazOrbit()`, `BOBBlazorLayout` mounted, `_Imports.razor` with the
> shared `@using` set, host page interactivity configured per "Hosting
> bootstrap traps"). Models referenced in `@code` blocks are kept terse — fill
> in real validation attributes / DataAnnotations as needed.

---

## 1. Marketing landing page

Hero + features grid + testimonials carousel + CTA.

```razor
@page "/"
@layout MainLayout

<BOBFlexStack Direction="FlexStackDirection.Column"
              Gap="6rem"
              FullWidth="true"
              Pb="6rem">

    @* ---- Hero ---- *@
    <section style="padding: 6rem 1.5rem; text-align: center;
                    background: radial-gradient(ellipse at top,
                        color-mix(in oklab, var(--palette-primary) 18%, transparent),
                        var(--palette-background) 60%);">
        <BOBGrid MaxWidth="960px" Gap="1.5rem" Direction="GridDirection.Column"
                 AlignItems="GridAlignItems.Center">
            <h1 style="font-size: clamp(2.25rem, 4vw + 1rem, 4rem);
                       line-height: 1.05;
                       margin: 0;
                       color: var(--palette-background-contrast);
                       font-family: var(--bob-font-family-heading);">
                Ship faster with a design system that gets out of your way.
            </h1>
            <p style="max-width: 56ch; margin: 0; font-size: 1.15rem;
                      color: color-mix(in oklab, var(--palette-background-contrast) 70%, transparent);">
                BlazOrbit gives your Blazor app 50+ accessible components, a
                token-first theme, and zero JS framework churn.
            </p>
            <BOBFlexStack Direction="FlexStackDirection.Row" Gap="0.75rem">
                <BOBButton Text="Start building"
                           Size="BOBSize.Large"
                           BackgroundColor="@PaletteColor.Primary"
                           Color="@PaletteColor.PrimaryContrast"
                           TrailingIcon="@BOBIconKeys.UI.ArrowForward"
                           Shadow="@BOBShadowPresets.Elevation(4)"
                           Transitions="@BOBTransitionPresets.PremiumButton"
                           OnClick="@(() => Nav.NavigateTo("/signup"))" />
                <BOBButton Text="View docs"
                           Size="BOBSize.Large"
                           Border="@BOBBorderPresets.Subtle"
                           OnClick="@(() => Nav.NavigateTo("/docs"))" />
            </BOBFlexStack>
        </BOBGrid>
    </section>

    @* ---- Feature grid ---- *@
    <section>
        <BOBGrid Columns="3" DirectionMd="GridDirection.Row"
                 DirectionXs="GridDirection.Column"
                 Gap="1.5rem"
                 MaxWidth="1100px"
                 Px="1.5rem">
            @foreach (var f in _features)
            {
                <BOBGridItem>
                    <BOBCard Elevation="1"
                             Border="@BOBBorderPresets.RoundedLarge"
                             BackgroundColor="@PaletteColor.Surface"
                             Color="@PaletteColor.SurfaceContrast"
                             Transitions="@BOBTransitionPresets.CardHover">
                        <Header>
                            <BOBSvgIcon Title="@f.Title"
                                        Color="@PaletteColor.Primary"
                                        Size="BOBSize.Large">
                                @f.Icon.SvgContent
                            </BOBSvgIcon>
                        </Header>
                        <ChildContent>
                            <h3 style="margin: 0 0 0.5rem;">@f.Title</h3>
                            <p style="margin: 0; color: color-mix(in oklab, currentColor 70%, transparent);">
                                @f.Body
                            </p>
                        </ChildContent>
                    </BOBCard>
                </BOBGridItem>
            }
        </BOBGrid>
    </section>

    @* ---- Testimonials carousel ---- *@
    <section style="background: var(--palette-surface);
                    padding: 4rem 1.5rem;">
        <BOBCarousel AutoPlay="true"
                     AutoPlayInterval="@TimeSpan.FromSeconds(8)"
                     Loop="true"
                     ShowIndicators="true"
                     ShowArrows="true"
                     Transition="BOBCarouselTransition.Fade"
                     TransitionDurationMs="600">
            @foreach (var t in _quotes)
            {
                <BOBCarouselItem>
                    <BOBGrid Direction="GridDirection.Column"
                             AlignItems="GridAlignItems.Center"
                             Gap="1rem"
                             MaxWidth="640px"
                             Px="1.5rem">
                        <p style="font-size: 1.5rem; line-height: 1.4; text-align: center;
                                  color: var(--palette-surface-contrast);">
                            "@t.Quote"
                        </p>
                        <span style="opacity: 0.7;">— @t.Author, @t.Role</span>
                    </BOBGrid>
                </BOBCarouselItem>
            }
        </BOBCarousel>
    </section>

    @* ---- Final CTA ---- *@
    <section style="text-align: center; padding: 0 1.5rem;">
        <BOBCard Elevation="3"
                 Border="@BOBBorderPresets.RoundedLarge"
                 BackgroundColor="@PaletteColor.Primary"
                 Color="@PaletteColor.PrimaryContrast"
                 ActionsAlignment="CardActionsAlignment.Center">
            <ChildContent>
                <h2 style="margin: 0 0 0.5rem;">Ready when you are</h2>
                <p style="margin: 0; opacity: 0.85;">
                    Free tier, no credit card, MIT-licensed components.
                </p>
            </ChildContent>
            <Actions>
                <BOBButton Text="Get started"
                           BackgroundColor="@PaletteColor.PrimaryContrast"
                           Color="@PaletteColor.Primary"
                           Size="BOBSize.Large"
                           OnClick="@(() => Nav.NavigateTo("/signup"))" />
            </Actions>
        </BOBCard>
    </section>

</BOBFlexStack>

@inject NavigationManager Nav

@code {
    private record Feature(string Title, string Body, IconKey Icon);
    private record Quote(string Quote, string Author, string Role);

    private readonly Feature[] _features =
    [
        new("Tokens, not classes",
            "Override `--palette-*` and `--bob-*` to skin every component instantly.",
            BOBIconKeys.UI.Tune),
        new("Accessibility built in",
            "WCAG 2.2 AA out of the box: focus rings, ARIA wiring, RTL aware.",
            BOBIconKeys.UI.AccessTime),
        new("Stays out of the way",
            "No CSS-in-JS, no PurgeCSS, no framework reset to fight.",
            BOBIconKeys.UI.Check),
    ];

    private readonly Quote[] _quotes =
    [
        new("Cut our component layer in half.", "A. Müller", "Tech lead, Acme"),
        new("Theme switching just works.", "S. Patel", "Founder, Northwind"),
    ];
}
```

---

## 2. Admin dashboard

Sidebar layout + KPI cards + datagrid + drawer for filters.

```razor
@page "/admin"
@layout MainLayout
@inject IModalService ModalService

<BOBSidebarLayout SidebarSide="SidebarSide.Start"
                  SidebarWidth="240px"
                  HeaderHeight="56px"
                  StickyHeader="true"
                  StickySidebar="true">
    <Header>
        <BOBFlexStack Direction="FlexStackDirection.Row"
                      AlignItems="FlexStackAlignItems.Center"
                      JustifyContent="FlexStackJustifyContent.SpaceBetween"
                      FullWidth="true"
                      Px="1rem">
            <strong>Admin</strong>
            <BOBFlexStack Direction="FlexStackDirection.Row" Gap="0.5rem">
                <BOBButton LeadingIcon="@BOBIconKeys.UI.FilterList"
                           Text="Filters"
                           Variant="BOBButtonVariant.Default"
                           Border="@BOBBorderPresets.Subtle"
                           OnClick="OpenFiltersAsync" />
                <BOBThemeSelector />
            </BOBFlexStack>
        </BOBFlexStack>
    </Header>
    <Sidebar>
        <BOBTreeMenu TItem="NavItem" Items="_nav"
                     KeySelector="n => n.Key"
                     ChildrenSelector="n => n.Children"
                     OnNavigate="key => Nav.NavigateTo(key)" />
    </Sidebar>
    <ChildContent>
        <BOBFlexStack Direction="FlexStackDirection.Column" Gap="1.5rem" P="1.5rem">

            @* ---- KPI row ---- *@
            <BOBGrid Columns="4" GapMd="1rem" GapXs="0.5rem">
                @foreach (var k in _kpis)
                {
                    <BOBGridItem Sm="6" Md="3">
                        <BOBCard Elevation="1" Border="@BOBBorderPresets.Rounded">
                            <Header>
                                <span style="opacity: 0.65; font-size: 0.875rem;">@k.Label</span>
                            </Header>
                            <ChildContent>
                                <BOBFlexStack Direction="FlexStackDirection.Row"
                                              AlignItems="FlexStackAlignItems.Baseline"
                                              Gap="0.5rem">
                                    <span style="font-size: 1.75rem; font-weight: 600;">@k.Value</span>
                                    <BOBBadge Size="BOBSize.Small"
                                              BackgroundColor="@(k.Delta >= 0 ? PaletteColor.Success : PaletteColor.Error)"
                                              Color="@(k.Delta >= 0 ? PaletteColor.SuccessContrast : PaletteColor.ErrorContrast)">
                                        @(k.Delta >= 0 ? "+" : "")@k.Delta%
                                    </BOBBadge>
                                </BOBFlexStack>
                            </ChildContent>
                        </BOBCard>
                    </BOBGridItem>
                }
            </BOBGrid>

            @* ---- Records table ---- *@
            <BOBCard Elevation="1" Border="@BOBBorderPresets.Rounded">
                <Header><h3 style="margin: 0;">Recent users</h3></Header>
                <ChildContent>
                    <BOBDataGrid TItem="UserDto"
                                 Items="_users"
                                 Hoverable="true"
                                 FixedHeader="true"
                                 PageSize="10"
                                 Sortable="true"
                                 Filterable="true"
                                 Density="BOBDensity.Compact"
                                 EmptyContent="@(__ => @<p>No users found.</p>)"
                                 OnRowClick="OpenUserAsync">
                        <Columns>
                            <BOBDataColumn TItem="UserDto"
                                           Property="u => u.Email"
                                           Header="Email"
                                           Sortable="true"
                                           Filterable="true" />
                            <BOBDataColumn TItem="UserDto"
                                           Property="u => u.JoinedAt"
                                           Header="Joined"
                                           Format="yyyy-MM-dd"
                                           Align="ColumnAlign.Right" />
                            <BOBDataColumn TItem="UserDto"
                                           Header="Status">
                                <Template Context="u">
                                    <BOBBadge BackgroundColor="@(u.IsActive ? PaletteColor.Success : PaletteColor.Border)"
                                              Color="@(u.IsActive ? PaletteColor.SuccessContrast : PaletteColor.SurfaceContrast)">
                                        @(u.IsActive ? "Active" : "Disabled")
                                    </BOBBadge>
                                </Template>
                            </BOBDataColumn>
                        </Columns>
                    </BOBDataGrid>
                </ChildContent>
            </BOBCard>
        </BOBFlexStack>
    </ChildContent>
</BOBSidebarLayout>

@inject NavigationManager Nav

@code {
    private record NavItem(string Key, string Label, IEnumerable<NavItem>? Children = null);
    private record Kpi(string Label, string Value, int Delta);
    private record UserDto(string Email, DateOnly JoinedAt, bool IsActive);

    private readonly NavItem[] _nav =
    [
        new("/admin",        "Overview"),
        new("/admin/users",  "Users"),
        new("/admin/events", "Events"),
        new("/admin/billing","Billing"),
    ];
    private readonly Kpi[] _kpis =
    [
        new("MRR",           "$48,210", 12),
        new("Active users",  "3,418",   4),
        new("Churn",         "1.2%",   -3),
        new("NPS",           "62",      0),
    ];
    private List<UserDto> _users = new(); // populate from API

    private Task OpenFiltersAsync() =>
        ModalService.ShowDrawerAsync<FiltersDrawer>(
            options: new DrawerOptions { Position = DrawerPosition.Right, Size = "360px" });

    private Task OpenUserAsync(UserDto u) =>
        ModalService.ShowDialogAsync<EditUserDialog>(
            parameters: new { Email = u.Email },
            options: new DialogOptions { Title = "Edit user", MaxWidth = "560px" });
}
```

`FiltersDrawer.razor` and `EditUserDialog.razor` implement `IModalContent` —
see SKILL.md → "IModalContent + ModalReference — full contract".

---

## 3. Login form

Submit-friendly: dedicated `<button type="submit">` so `EditForm.OnValidSubmit` fires.

```razor
@page "/login"
@inject AuthService Auth
@inject IToastService ToastService
@inject NavigationManager Nav

<BOBFlexStack Direction="FlexStackDirection.Column"
              AlignItems="FlexStackAlignItems.Center"
              JustifyContent="FlexStackJustifyContent.Center"
              FullWidth="true"
              Pt="6rem" Pb="3rem" Px="1.5rem">

    <BOBCard Elevation="3"
             Border="@BOBBorderPresets.RoundedLarge"
             BackgroundColor="@PaletteColor.Surface"
             Color="@PaletteColor.SurfaceContrast">
        <Header>
            <h2 style="margin: 0;">Welcome back</h2>
            <p style="margin: 0.25rem 0 0; opacity: 0.7;">Sign in to continue</p>
        </Header>
        <ChildContent>
            <EditForm Model="_model" OnValidSubmit="HandleSubmitAsync">
                <DataAnnotationsValidator />

                <BOBFlexStack Direction="FlexStackDirection.Column" Gap="1rem"
                              Style="min-width: min(420px, 80vw);">
                    <BOBInputText @bind-Value="_model.Email"
                                  Label="Email"
                                  Variant="BOBInputVariant.Outlined"
                                  Required="true"
                                  PrefixIcon="@BOBIconKeys.UI.Email"
                                  AriaLabel="Email" />

                    <BOBInputText @bind-Value="_model.Password"
                                  Label="Password"
                                  Variant="BOBInputVariant.Outlined"
                                  Required="true"
                                  PrefixIcon="@BOBIconKeys.UI.Person"
                                  HelperText="At least 8 characters"
                                  AdditionalAttributes="@(new Dictionary<string, object>{ ["type"] = "password" })" />

                    <BOBInputCheckbox @bind-Value="_model.RememberMe"
                                      Label="Keep me signed in"
                                      Color="@PaletteColor.Primary" />

                    <button type="submit"
                            disabled="@_busy"
                            style="all: unset; width: 100%;">
                        <BOBButton Text="@(_busy ? "Signing in…" : "Sign in")"
                                   FullWidth="true"
                                   Size="BOBSize.Large"
                                   BackgroundColor="@PaletteColor.Primary"
                                   Color="@PaletteColor.PrimaryContrast"
                                   Loading="@_busy"
                                   Disabled="@_busy" />
                    </button>
                </BOBFlexStack>
            </EditForm>
        </ChildContent>
        <Actions>
            <a href="/forgot" style="color: var(--palette-primary);">Forgot password?</a>
            <a href="/signup" style="color: var(--palette-primary);">Create account</a>
        </Actions>
    </BOBCard>
</BOBFlexStack>

@code {
    private LoginModel _model = new();
    private bool _busy;

    private async Task HandleSubmitAsync()
    {
        _busy = true;
        try
        {
            var ok = await Auth.SignInAsync(_model.Email, _model.Password, _model.RememberMe);
            if (ok) Nav.NavigateTo("/admin");
            else
            {
                await ToastService.ShowAsync<ErrorToast>(
                    new Dictionary<string, object?> { ["Message"] = "Invalid credentials." },
                    ToastOptions.Default with { Severity = ToastSeverity.Error });
            }
        }
        finally { _busy = false; }
    }

    private sealed class LoginModel
    {
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MinLength(8)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}
```

The wrapper `<button type="submit">` with `all: unset` lets the styled
`BOBButton` look the same while still triggering `EditForm.OnValidSubmit`.
Drop the wrapper if you don't need form-submission semantics (e.g. you call
`Auth.SignInAsync` from `OnClick` directly and skip `EditForm`).

---

## 4. Multi-step signup

Tabs as steps + per-step validation + summary preview.

```razor
@page "/signup"

<BOBFlexStack Direction="FlexStackDirection.Column"
              AlignItems="FlexStackAlignItems.Center"
              FullWidth="true" Pt="3rem" Pb="6rem" Px="1.5rem">

    <BOBCard Elevation="2" Border="@BOBBorderPresets.RoundedLarge"
             Style="width: min(640px, 100%);">
        <Header><h2 style="margin: 0;">Create your account</h2></Header>
        <ChildContent>
            <EditForm Model="_model" OnValidSubmit="FinishAsync">
                <DataAnnotationsValidator />

                <BOBTabs @bind-ActiveTab="_step"
                         Variant="BOBTabsVariant.Pills"
                         FullWidth="true">
                    <BOBTab Id="account" Label="Account">
                        <BOBFlexStack Direction="FlexStackDirection.Column" Gap="1rem" Pt="1rem">
                            <BOBInputText @bind-Value="_model.Email"
                                          Label="Email" Required="true"
                                          PrefixIcon="@BOBIconKeys.UI.Email" />
                            <BOBInputText @bind-Value="_model.Password"
                                          Label="Password" Required="true"
                                          AdditionalAttributes="@(new Dictionary<string, object>{ ["type"] = "password" })"
                                          HelperText="≥ 8 chars, mixed case + digit." />
                        </BOBFlexStack>
                    </BOBTab>

                    <BOBTab Id="profile" Label="Profile">
                        <BOBFlexStack Direction="FlexStackDirection.Column" Gap="1rem" Pt="1rem">
                            <BOBInputText @bind-Value="_model.FullName" Label="Full name" Required="true" />
                            <BOBInputDateTime @bind-Value="_model.DateOfBirth" Label="Date of birth" />
                            <BOBInputDropdown TValue="string" @bind-Value="_model.Country" Label="Country" Searchable="true">
                                @foreach (var c in _countries)
                                {
                                    <DropdownOption Value="@c.Code">@c.Name</DropdownOption>
                                }
                            </BOBInputDropdown>
                        </BOBFlexStack>
                    </BOBTab>

                    <BOBTab Id="confirm" Label="Confirm">
                        <BOBFlexStack Direction="FlexStackDirection.Column" Gap="0.75rem" Pt="1rem">
                            <p>Please confirm:</p>
                            <ul style="margin: 0; padding-left: 1.25rem;">
                                <li>Email: <strong>@_model.Email</strong></li>
                                <li>Name: <strong>@_model.FullName</strong></li>
                                <li>Country: <strong>@_model.Country</strong></li>
                            </ul>
                            <BOBInputCheckbox @bind-Value="_model.AcceptTerms"
                                              Label="I accept the terms"
                                              Required="true" />
                        </BOBFlexStack>
                    </BOBTab>
                </BOBTabs>

                <BOBFlexStack Direction="FlexStackDirection.Row"
                              JustifyContent="FlexStackJustifyContent.SpaceBetween"
                              Pt="1.5rem">
                    <BOBButton Text="Back"
                               Disabled="@(_step == "account")"
                               OnClick="GoBack" />
                    @if (_step == "confirm")
                    {
                        <button type="submit" style="all: unset;">
                            <BOBButton Text="Create account"
                                       BackgroundColor="@PaletteColor.Primary"
                                       Color="@PaletteColor.PrimaryContrast" />
                        </button>
                    }
                    else
                    {
                        <BOBButton Text="Next"
                                   BackgroundColor="@PaletteColor.Primary"
                                   Color="@PaletteColor.PrimaryContrast"
                                   OnClick="GoNext" />
                    }
                </BOBFlexStack>
            </EditForm>
        </ChildContent>
    </BOBCard>
</BOBFlexStack>

@code {
    private string _step = "account";
    private SignupModel _model = new();

    private readonly (string Code, string Name)[] _countries =
    [
        ("ES", "Spain"), ("DE", "Germany"), ("US", "United States"),
    ];

    private void GoNext() => _step = _step switch
    {
        "account" => "profile",
        "profile" => "confirm",
        _         => _step,
    };
    private void GoBack() => _step = _step switch
    {
        "confirm" => "profile",
        "profile" => "account",
        _         => _step,
    };

    private Task FinishAsync() => Task.CompletedTask; // wire to API

    private sealed class SignupModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Email { get; set; } = "";
        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = "";
        [System.ComponentModel.DataAnnotations.Required]
        public string FullName { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public string Country { get; set; } = "";
        public bool AcceptTerms { get; set; }
    }
}
```

The "step gate" is the simple `_step` switch. A more rigorous flow validates
the current tab's bound fields before advancing — call
`EditContext.Validate()` on a per-step `EditContext` if you need that.

---

## 5. Contact form with toast feedback

Single-screen form, hide it on success, fire toast.

```razor
@page "/contact"
@inject IToastService ToastService

<BOBFlexStack Direction="FlexStackDirection.Column"
              AlignItems="FlexStackAlignItems.Center"
              FullWidth="true" Pt="3rem" Pb="6rem" Px="1.5rem">

    <BOBCard Elevation="2" Border="@BOBBorderPresets.RoundedLarge"
             Style="width: min(560px, 100%);">
        <Header><h2 style="margin: 0;">Contact us</h2></Header>
        <ChildContent>
            <EditForm Model="_model" OnValidSubmit="SendAsync">
                <DataAnnotationsValidator />
                <BOBFlexStack Direction="FlexStackDirection.Column" Gap="1rem">
                    <BOBInputText @bind-Value="_model.Name" Label="Your name" Required="true" />
                    <BOBInputText @bind-Value="_model.Email" Label="Email" Required="true" />
                    <BOBInputDropdown TValue="string" @bind-Value="_model.Topic" Label="Topic">
                        @foreach (var t in new[] { "Sales", "Support", "Press" })
                        {
                            <DropdownOption Value="@t">@t</DropdownOption>
                        }
                    </BOBInputDropdown>
                    <BOBInputTextArea @bind-Value="_model.Message"
                                      Label="How can we help?"
                                      Required="true"
                                      Rows="5"
                                      AutoResize="true"
                                      MaxLength="2000"
                                      HelperText="Plain text. We'll reply within 24h." />
                    <button type="submit" style="all: unset;">
                        <BOBButton Text="@(_busy ? "Sending…" : "Send")"
                                   FullWidth="true"
                                   Loading="@_busy"
                                   Disabled="@_busy"
                                   BackgroundColor="@PaletteColor.Primary"
                                   Color="@PaletteColor.PrimaryContrast" />
                    </button>
                </BOBFlexStack>
            </EditForm>
        </ChildContent>
    </BOBCard>
</BOBFlexStack>

@code {
    private ContactModel _model = new();
    private bool _busy;

    private async Task SendAsync()
    {
        _busy = true;
        try
        {
            // POST _model to API
            await Task.Delay(800);
            _model = new();
            await ToastService.ShowAsync<MessageToast>(
                new Dictionary<string, object?> { ["Message"] = "Thanks — we'll be in touch." },
                ToastOptions.Default with { Severity = ToastSeverity.Success, Duration = TimeSpan.FromSeconds(4) });
        }
        finally { _busy = false; }
    }

    private sealed class ContactModel
    {
        [System.ComponentModel.DataAnnotations.Required] public string Name { get; set; } = "";
        [System.ComponentModel.DataAnnotations.Required] public string Email { get; set; } = "";
        public string Topic { get; set; } = "Sales";
        [System.ComponentModel.DataAnnotations.Required] public string Message { get; set; } = "";
    }
}
```

`MessageToast.razor` is a tiny body component:

```razor
@code {
    [Parameter] public string Message { get; set; } = "";
}

<strong>@Message</strong>
```

---

## 6. Pricing table

Three tiers, "popular" emphasis, palette-driven.

```razor
@page "/pricing"

<BOBFlexStack Direction="FlexStackDirection.Column" Gap="2rem"
              AlignItems="FlexStackAlignItems.Center"
              FullWidth="true" Pt="4rem" Pb="6rem" Px="1.5rem">

    <BOBGrid Direction="GridDirection.Column" Gap="0.5rem"
             AlignItems="GridAlignItems.Center" MaxWidth="640px">
        <h1 style="margin: 0;">Plans that scale with you</h1>
        <p style="margin: 0; opacity: 0.7;">Switch tiers anytime — no lock-in.</p>
    </BOBGrid>

    <BOBGrid Columns="3" GapMd="1.5rem" GapXs="1rem"
             DirectionXs="GridDirection.Column"
             DirectionMd="GridDirection.Row"
             MaxWidth="1100px">
        @foreach (var p in _plans)
        {
            <BOBGridItem Sm="12" Md="4">
                <BOBCard Elevation="@(p.Popular ? 6 : 1)"
                         Border="@(p.Popular ? BOBBorderPresets.Primary : BOBBorderPresets.Subtle)"
                         BackgroundColor="@(p.Popular ? PaletteColor.Primary : PaletteColor.Surface)"
                         Color="@(p.Popular ? PaletteColor.PrimaryContrast : PaletteColor.SurfaceContrast)"
                         Transitions="@BOBTransitionPresets.HoverLift">
                    <Header>
                        <BOBFlexStack Direction="FlexStackDirection.Row"
                                      JustifyContent="FlexStackJustifyContent.SpaceBetween"
                                      AlignItems="FlexStackAlignItems.Center">
                            <h3 style="margin: 0;">@p.Name</h3>
                            @if (p.Popular)
                            {
                                <BOBBadge BackgroundColor="@PaletteColor.PrimaryContrast"
                                          Color="@PaletteColor.Primary"
                                          Size="BOBSize.Small">Popular</BOBBadge>
                            }
                        </BOBFlexStack>
                    </Header>
                    <ChildContent>
                        <p style="font-size: 2.25rem; font-weight: 600; margin: 0.5rem 0;">
                            @p.Price<span style="font-size: 1rem; opacity: 0.7;">/mo</span>
                        </p>
                        <ul style="list-style: none; padding: 0; margin: 0;">
                            @foreach (var f in p.Features)
                            {
                                <li style="padding: 0.25rem 0;">
                                    <BOBSvgIcon Color="@(p.Popular ? PaletteColor.PrimaryContrast : PaletteColor.Success)"
                                                Size="BOBSize.Small">
                                        @BOBIconKeys.UI.Check.SvgContent
                                    </BOBSvgIcon>
                                    @f
                                </li>
                            }
                        </ul>
                    </ChildContent>
                    <Actions>
                        <BOBButton Text="@(p.Popular ? "Get started" : "Choose")"
                                   FullWidth="true"
                                   BackgroundColor="@(p.Popular ? PaletteColor.PrimaryContrast : PaletteColor.Primary)"
                                   Color="@(p.Popular ? PaletteColor.Primary : PaletteColor.PrimaryContrast)" />
                    </Actions>
                </BOBCard>
            </BOBGridItem>
        }
    </BOBGrid>

</BOBFlexStack>

@code {
    private record Plan(string Name, string Price, string[] Features, bool Popular);

    private readonly Plan[] _plans =
    [
        new("Free",  "$0",  ["Up to 3 projects", "Community support", "Core components"], false),
        new("Pro",   "$19", ["Unlimited projects", "Email support", "Premium variants", "Custom themes"], true),
        new("Team",  "$49", ["Everything in Pro", "SSO", "Priority support", "Audit log"], false),
    ];
}
```

---

## 7. Settings page (sidebar + tabs + form)

Long-form editing with section navigation.

```razor
@page "/settings"

<BOBSidebarLayout SidebarSide="SidebarSide.Start"
                  SidebarWidth="220px"
                  StickyHeader="true">
    <Header><strong style="padding: 0 1rem;">Settings</strong></Header>
    <Sidebar>
        <BOBTreeMenu TItem="string" Items="@(new[] { "Account", "Notifications", "Appearance", "Billing" })"
                     KeySelector="s => s"
                     OnNodeClick="ctx => _section = ctx.Node.Item"
                     ChildrenSelector="_ => null" />
    </Sidebar>
    <ChildContent>
        <BOBFlexStack Direction="FlexStackDirection.Column" Gap="1.5rem" P="1.5rem">

            @if (_section == "Account")
            {
                <BOBCard Elevation="1">
                    <Header><h3 style="margin: 0;">Profile</h3></Header>
                    <ChildContent>
                        <EditForm Model="_profile" OnValidSubmit="SaveProfileAsync">
                            <DataAnnotationsValidator />
                            <BOBFlexStack Direction="FlexStackDirection.Column" Gap="1rem">
                                <BOBInputText @bind-Value="_profile.DisplayName" Label="Display name" Required="true" />
                                <BOBInputText @bind-Value="_profile.Email"       Label="Email"        Required="true" />
                                <BOBInputDateTime @bind-Value="_profile.Birthday" Label="Birthday" />
                                <button type="submit" style="all: unset;">
                                    <BOBButton Text="Save profile"
                                               BackgroundColor="@PaletteColor.Primary"
                                               Color="@PaletteColor.PrimaryContrast" />
                                </button>
                            </BOBFlexStack>
                        </EditForm>
                    </ChildContent>
                </BOBCard>
            }
            else if (_section == "Appearance")
            {
                <BOBCard Elevation="1">
                    <Header><h3 style="margin: 0;">Theme</h3></Header>
                    <ChildContent>
                        <BOBFlexStack Direction="FlexStackDirection.Column" Gap="0.75rem">
                            <BOBThemeSelector />
                            <BOBSwitch TValue="bool" @bind-Value="_compact" Label="Compact density" />
                            <BOBSwitch TValue="bool" @bind-Value="_motion"  Label="Reduce motion" />
                        </BOBFlexStack>
                    </ChildContent>
                </BOBCard>
            }
            else if (_section == "Notifications")
            {
                @* Inline checkbox list, validation, save  *@
            }
        </BOBFlexStack>
    </ChildContent>
</BOBSidebarLayout>

@code {
    private string _section = "Account";
    private bool _compact, _motion;
    private ProfileModel _profile = new();

    private Task SaveProfileAsync() => Task.CompletedTask;

    private sealed class ProfileModel
    {
        [System.ComponentModel.DataAnnotations.Required] public string DisplayName { get; set; } = "";
        [System.ComponentModel.DataAnnotations.Required] public string Email       { get; set; } = "";
        public DateTime? Birthday { get; set; }
    }
}
```

`BOBSwitch<TValue>` here is the **presentation** switch (no validation, no
`EditForm` integration). For switches that participate in form validation,
use `BOBInputSwitch` inside an `EditForm`.

---

## See also

- `references/components.md` — every parameter, including inherited ones.
- `references/icons.md` — full `BOBIconKeys` catalog.
- `references/theming.md` — token + palette overrides for themes.
- `references/presets.md` — `BOBBorderPresets`, `BOBShadowPresets`, `BOBTransitionPresets`, `ToastOptions`, `DialogOptions`.
- `references/patterns.md` — small per-component snippets.
- SKILL.md → "Decision matrix" — when to pick which component.
- SKILL.md → "Hosting bootstrap traps" — Server vs Wasm interactivity setup.
- SKILL.md → "IModalContent + ModalReference" — body component contract for `IModalService`.
