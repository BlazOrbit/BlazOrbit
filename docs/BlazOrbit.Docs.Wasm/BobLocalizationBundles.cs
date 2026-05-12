using BlazOrbit.Localization;

// Bundle declarations for every Razor page in the docs site that injects
// IStringLocalizer<TPage>. Each marker is the page's auto-generated partial class. The
// source generator (BlazOrbit.Localization.CodeGeneration) reads these attributes, pairs
// each marker with the matching .tn files under Translations/<RelativeNamespace>/<PageName>.<culture>.tn,
// and emits the runtime registration via [ModuleInitializer] so bundles are available
// before any consumer resolves IStringLocalizer<TPage>.
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Layout.HomeLayout), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Layout.MainLayout), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Layout.NavMenu), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.AccordionPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.AspectRatioPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.AutoCompletePage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.AvatarPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.BadgePage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.BannerPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.BreadcrumbsPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.ButtonPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.CardPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.CarouselPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.AreaChartPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.BarChartPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.BoxplotChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.CandlestickChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.ChartsPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.DonutChartPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.FilterContextPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.FunnelChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.GaugeChartPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.HeatmapChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.HistogramChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.LineChartPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.MixedChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.PieChartPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.PolarAreaChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.RadarChartPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.SankeyChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.ScatterChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.SparklinePage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.StockChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.SunburstChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Charts.TreemapChartPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.CheckboxPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.ChipPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.CodeBlockPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.ColorPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.ConfirmServicePage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.ContainerPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.DataCardsPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.DataGridPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.DateRangePage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.DateTimePage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.DraggablePage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.DropdownPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.FileUploadPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.FlexStackPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.FormsPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.GridPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.HotkeyServicePage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.InputNumberSliderPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.InputRangeSliderPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.InputTextPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.ModalPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.NotificationCenterPage),
        DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.NumberPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.OtpPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.PageHeaderPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.PasswordPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.ProgressPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.RadioPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.RatingPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.SplitterPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.StatCardPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.StepperPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.SvgIconPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.SwitchPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.TabsPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.Forms.TextAreaPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.ThemeSelectorPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.TimelinePage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.ToastPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.TooltipPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.TreeMenuPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Components.TreeSelectorPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Concepts.Accessibility), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Concepts.Theming), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Concepts.Variants), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Features.IconsGallery), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Features.Localization), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Features.ValidationPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.GettingStarted.AiSkill), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.GettingStarted.Index), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.GettingStarted.Templates), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Home), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.NotFound), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Other.LiveDevelopmentPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Privacy), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Search), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Pages.Utils.ThemeGeneratorPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Shared.DocFooter), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Shared.DocPage), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Shared.DocSearch), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Shared.DocSeo), DefaultCulture = "en-US")]
[assembly:
    BobLocalizationBundle(typeof(BlazOrbit.Docs.Wasm.Shared.InstallAppHint), DefaultCulture = "en-US")]
