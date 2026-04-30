using BlazOrbit.Components;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlazOrbit.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class TransitionsCssGenerator : IAssetGenerator
{
    private static readonly (string Name, string ChildPseudo, string SelfPseudo)[] TriggerPseudos =
    [
        ("hover",  ":hover:not(:has(:disabled))",  ":hover:not(:disabled)"),
        ("focus",  ":focus-within",                 ":focus-visible"),
        ("active", ":active:not(:has(:disabled))",  ":active:not(:disabled)")
    ];

    public string FileName => "_transition-classes.css";
    public string Name => "Transitions CSS";

    public Task<string> GetContent()
    {
        string tag = FeatureDefinitions.Tags.Component;
        string attr = FeatureDefinitions.DataAttributes.Transitions;
        string target = FeatureDefinitions.Tokens.Transitions.TargetClass;
        string shorthand = FeatureDefinitions.Tokens.Transitions.Shorthand;

        StringBuilder sb = new();

        sb.AppendLine($$"""
/* ========================================
   Transition Classes
   Auto-generated - Do not edit manually
   ======================================== */

/* === BASE: apply transition shorthand to target element === */

{{tag}}[{{attr}}] .{{target}},
{{tag}}[{{attr}}].{{target}} {
    transition: var({{shorthand}});
}
""");

        foreach ((string triggerName, string childPseudo, string selfPseudo) in TriggerPseudos)
        {
            sb.AppendLine($"/* === {triggerName.ToUpperInvariant()} === */");
            sb.AppendLine();

            foreach (string prop in FeatureDefinitions.Tokens.Transitions.Props)
            {
                string token = $"{triggerName}:{prop}";
                string variable = FeatureDefinitions.Tokens.Transitions.VariableFor(triggerName, prop);

                sb.AppendLine(
$$"""
{{tag}}[{{attr}}~="{{token}}"]{{childPseudo}} .{{target}},
{{tag}}[{{attr}}~="{{token}}"].{{target}}{{selfPseudo}} {
    {{prop}}: var({{variable}});
}
""");
            }
        }

        return Task.FromResult(sb.ToString());
    }
}
