namespace BlazOrbit.Components;

public interface IVariant
{
    string Name { get; }
}

public abstract class Variant : IVariant
{
    protected Variant(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        NameLower = name.ToLowerInvariant();
    }

    public string Name { get; }

    internal string NameLower { get; }

    public override bool Equals(object? obj)
        => obj is Variant other &&
        GetType() == other.GetType() &&
        Name == other.Name;

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Name);

    public override string ToString() => Name;
}