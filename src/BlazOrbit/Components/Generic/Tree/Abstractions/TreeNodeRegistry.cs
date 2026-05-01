namespace BlazOrbit.Components;

/// <summary>Cascading-parameter contract for tree containers that accept child node registrations.</summary>
public interface ITreeNodeRegistry<TRegistration>
    where TRegistration : TreeNodeRegistration
{
    /// <summary>Registers a child node with the container.</summary>
    void Register(TRegistration registration);
}

internal sealed class TreeNodeRegistry<TRegistration> : ITreeNodeRegistry<TRegistration>
    where TRegistration : TreeNodeRegistration
{
    private readonly List<TRegistration> _registrations = [];

    public void Clear() => _registrations.Clear();

    public IReadOnlyList<TRegistration> GetRegistrations()
        => _registrations.ToList();

    public void Register(TRegistration registration)
                => _registrations.Add(registration);

    // Return a copy to prevent external modification or clear external by reference
}