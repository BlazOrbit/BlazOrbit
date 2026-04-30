namespace BlazOrbit.Components;

public interface ITreeNodeRegistry<TRegistration>
    where TRegistration : TreeNodeRegistration
{
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