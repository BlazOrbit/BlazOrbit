namespace BlazOrbit.Components.Layout;

/// <summary>Variant definition for the <c>BOBToast</c> component.</summary>
public sealed class BOBToastVariant : Variant
{
    /// <summary>The default toast variant.</summary>
    public static readonly BOBToastVariant Default = new("Default");

    private BOBToastVariant(string name) : base(name)
    {
    }

    /// <summary>Creates a custom variant identified by <paramref name="name"/>.</summary>
    public static BOBToastVariant Custom(string name) => new(name);
}