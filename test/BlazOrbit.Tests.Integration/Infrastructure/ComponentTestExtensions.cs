using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazOrbit.Tests.Integration.Infrastructure;

public static class ComponentTestExtensions
{
    public static string GetNormalizedMarkup<TComponent>(this IRenderedComponent<TComponent> fragment) where TComponent : IComponent
    {
        return fragment.Markup
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();
    }

    /// <summary>
    /// Finds the root bob-component element and asserts its data-bob-component attribute
    /// matches the expected kebab name. Returns the element for further assertions.
    /// </summary>
    public static IElement AssertBuiComponent<TComponent>(
        this IRenderedComponent<TComponent> cut,
        string expectedKebabName)
        where TComponent : IComponent
    {
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Be(expectedKebabName,
            because: $"component root must identify itself as '{expectedKebabName}'");
        return root;
    }

    /// <summary>
    /// Finds the root bob-component element and asserts the data-bob-component attribute
    /// contains the expected kebab fragment (useful for generated names with suffixes).
    /// </summary>
    public static IElement AssertBuiComponentContains<TComponent>(
        this IRenderedComponent<TComponent> cut,
        string kebabFragment)
        where TComponent : IComponent
    {
        IElement root = cut.Find("bob-component");
        root.GetAttribute("data-bob-component").Should().Contain(kebabFragment,
            because: $"component root name must contain '{kebabFragment}'");
        return root;
    }

    /// <summary>
    /// Renders <typeparamref name="TComponent"/> with a cascading <see cref="EditContext"/> built
    /// from <paramref name="model"/>, plus an optional <see cref="ValidationMessageStore"/> for
    /// triggering validation in tests.
    /// </summary>
    public static (IRenderedComponent<TComponent> cut, EditContext editContext, ValidationMessageStore messageStore)
        RenderWithEditForm<TComponent, TModel>(
            this BunitContext ctx,
            TModel model,
            Action<ComponentParameterCollectionBuilder<TComponent>>? parameters = null)
        where TComponent : IComponent
        where TModel : class
    {
        EditContext editContext = new(model);
        ValidationMessageStore messageStore = new(editContext);

        IRenderedComponent<TComponent> cut = ctx.Render<TComponent>(p =>
        {
            parameters?.Invoke(p);
            p.AddCascadingValue(editContext);
        });

        return (cut, editContext, messageStore);
    }
}