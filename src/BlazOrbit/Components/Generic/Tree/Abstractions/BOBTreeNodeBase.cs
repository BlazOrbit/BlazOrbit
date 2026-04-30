using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazOrbit.Components;

/// <summary>
/// Registration-only base for tree nodes (<c>BOBTreeMenuItem</c>, <c>BOBTreeSelectorItem</c>).
/// Intentionally inherits <see cref="ComponentBase"/> (not <c>BOBComponentBase</c>) because the
/// node registers itself with the enclosing tree container via cascading parameters and the
/// container renders the flattened structure. The node does not emit its own
/// <c>&lt;bob-component&gt;</c> root.
/// </summary>
public abstract class BOBTreeNodeBase<TRegistration> : ComponentBase
    where TRegistration : TreeNodeRegistration
{
    private bool _registered;

    /// <summary>Child nodes nested inside this node. Each child registers itself recursively against the same tree.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Arbitrary payload associated with the node and surfaced through the registration to consumers.</summary>
    [Parameter] public object? Data { get; set; }

    /// <summary>When <see langword="true" />, the node is rendered in a disabled state and ignores activation.</summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>Material icon name displayed alongside the node text.</summary>
    [Parameter] public IconKey? Icon { get; set; }

    /// <summary>When <see langword="true" />, the node is expanded the first time the tree renders.</summary>
    [Parameter] public bool InitiallyExpanded { get; set; }

    /// <summary>Stable identifier for the node. When omitted, a key is derived from the node's position in the tree.</summary>
    [Parameter] public string? Key { get; set; }

    /// <summary>Optional render fragment that replaces the default text/icon layout for this node.</summary>
    [Parameter] public RenderFragment? NodeContent { get; set; }

    /// <summary>Visible label of the node.</summary>
    [Parameter] public string? Text { get; set; }

    [CascadingParameter(Name = "ParentNodeKey")]
    internal string? ParentNodeKey { get; set; }

    [CascadingParameter(Name = "TreeNodeRegistry")]
    internal ITreeNodeRegistry<TRegistration>? Registry { get; set; }

    protected string ResolvedKey { get; private set; } = string.Empty;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Registry != null && ChildContent != null)
        {
            builder.OpenComponent<CascadingValue<string>>(0);
            builder.AddComponentParameter(1, "Value", ResolvedKey);
            builder.AddComponentParameter(2, "Name", "ParentNodeKey");
            builder.AddComponentParameter(3, "ChildContent", ChildContent);
            builder.CloseComponent();
        }
    }

    protected abstract TRegistration CreateRegistration();

    protected abstract string GenerateDefaultKey();

    protected override void OnInitialized() => ResolvedKey = Key ?? GenerateDefaultKey();

    protected override void OnParametersSet()
    {
        if (Registry != null && !_registered)
        {
            TRegistration registration = CreateRegistration();
            Registry.Register(registration);
            _registered = true;
        }
    }
}