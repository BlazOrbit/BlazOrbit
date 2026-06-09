namespace BlazOrbit.Components;

/// <summary>Marker contract for a named component variant.</summary>
public interface IVariant
{
    /// <summary>Stable identifier for the variant.</summary>
    string Name { get; }
}

/// <summary>Base type for strongly-typed component variants. Equality compares variant type and name.</summary>
public abstract class Variant : IVariant
{
    /// <summary>Constructs a variant with the given name.</summary>
    protected Variant(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        NameLower = name.ToLowerInvariant();
    }

    /// <inheritdoc />
    public string Name { get; }

    internal string NameLower { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Variant other &&
           GetType() == other.GetType() &&
           Name == other.Name;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(GetType(), Name);

    /// <inheritdoc />
    public override string ToString() => Name;
}
