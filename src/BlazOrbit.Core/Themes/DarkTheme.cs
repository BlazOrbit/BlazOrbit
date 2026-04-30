using BlazOrbit.Components;

namespace BlazOrbit.Themes;

/// <summary>
/// Dark theme — Carbon / Mineral. Technical, modern dark palette with mineral accents:
/// neutral carbon background, cool muted highlights, designed for low-fatigue prolonged reading.
/// </summary>
public sealed class DarkTheme : BOBThemePaletteBase
{
    public DarkTheme()
    {
        Id = "dark";
        Name = "Dark";

        Background = new CssColor("#121417");
        // Carbón neutro: oscuro profundo, sin matices azulados o marrones.

        BackgroundContrast = new CssColor("#ECEFF1");
        // Gris muy claro: lectura cómoda y limpia.

        Surface = new CssColor("#1E2126");
        // Gris grafito elevado: diferencia clara respecto al fondo base.

        SurfaceContrast = new CssColor("#ECEFF1");
        // Contraste estable para texto prolongado.

        Error = new CssColor("#CF6679");
        // Rojo rosado técnico: visible y moderno, evita el rojo clásico.

        ErrorContrast = new CssColor("#121417");
        // Mantiene equilibrio visual.

        Success = new CssColor("#4DD4A3");
        // Verde mineral frío: sensación de sistema confiable y tecnológico.

        SuccessContrast = new CssColor("#121417");
        // Contraste suficiente sin efecto glow.

        Warning = new CssColor("#E6B566");
        // Arena metálica: advertencia sofisticada, legible en dark mode.

        WarningContrast = new CssColor("#121417");
        // Oscuro para claridad inmediata.

        Info = new CssColor("#7AA2F7");
        // Azul lavanda técnico: informativo y moderno, no corporativo.

        InfoContrast = new CssColor("#121417");
        // Buen contraste sin romper la estética.

        Primary = new CssColor("#8AB4F8");
        // Azul mineral luminoso: foco principal claro y contemporáneo.

        PrimaryContrast = new CssColor("#121417");
        // Accesibilidad y sobriedad.

        Secondary = new CssColor("#C792EA");
        // Violeta tecnológico: creativo y distintivo, muy moderno.

        SecondaryContrast = new CssColor("#121417");
        // Lectura clara en elementos secundarios.

        Shadow = new CssColor("rgba(0,0,0,0.5)");
        // Sombra casi-negra semi-transparente: simula elevación real sobre fondo oscuro.
        // Un `#777777` sólido resultaría *más claro* que `Background` y daría efecto glow
        // en lugar de elevación. Elevation-overlay (tonal tint M3) queda como follow-up.

        Border = new CssColor("#EEEEEE");

        Highlight = new CssColor("#FFB74D");
        // Ámbar cálido: focus outline con contraste ≈6.5:1 sobre #121417. El rojo no
        // destaca lo suficiente en dark — WCAG 2.4.7 exige ≥3:1 para UI graphics.

        HoverTint = new CssColor("rgba(255,255,255,0.08)");
        ActiveTint = new CssColor("rgba(255,255,255,0.12)");
        // State-layer opacity (Material 3). Tinte claro semi-transparente sobre tema oscuro.
    }
}